using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace TaskTrackerAPI.Services.Auth
{
    internal static class TotpAuthenticator
    {
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static string CreateSecret()
        {
            return EncodeBase32(RandomNumberGenerator.GetBytes(20));
        }

        public static string CreateAuthenticatorUri(
            string secret,
            string accountName,
            string issuer)
        {
            var label = Uri.EscapeDataString($"{issuer}:{accountName}");
            var escapedIssuer = Uri.EscapeDataString(issuer);
            return $"otpauth://totp/{label}?secret={secret}&issuer={escapedIssuer}&algorithm=SHA1&digits=6&period=30";
        }

        public static bool Verify(string secret, string? code, DateTime utcNow)
        {
            return TryVerify(secret, code, utcNow, out _);
        }

        public static bool TryVerify(
            string secret,
            string? code,
            DateTime utcNow,
            out long matchedTimeStep)
        {
            matchedTimeStep = -1;
            if (code is null || code.Length != 6 || code.Any(c => c < '0' || c > '9'))
                return false;

            byte[] key;
            try
            {
                key = DecodeBase32(secret);
            }
            catch (FormatException)
            {
                return false;
            }

            var currentStep = new DateTimeOffset(utcNow).ToUnixTimeSeconds() / 30;
            var suppliedCode = Encoding.ASCII.GetBytes(code);

            // Allow one 30-second interval of clock drift in either direction.
            for (var drift = -1; drift <= 1; drift++)
            {
                var expectedCode = Encoding.ASCII.GetBytes(GenerateCode(key, currentStep + drift));
                if (CryptographicOperations.FixedTimeEquals(suppliedCode, expectedCode))
                {
                    matchedTimeStep = currentStep + drift;
                    return true;
                }
            }

            return false;
        }

        private static string GenerateCode(byte[] key, long timestep)
        {
            Span<byte> counter = stackalloc byte[sizeof(long)];
            BinaryPrimitives.WriteInt64BigEndian(counter, timestep);
            var digest = HMACSHA1.HashData(key, counter);
            var offset = digest[^1] & 0x0f;
            var binary = ((digest[offset] & 0x7f) << 24)
                | ((digest[offset + 1] & 0xff) << 16)
                | ((digest[offset + 2] & 0xff) << 8)
                | (digest[offset + 3] & 0xff);
            return (binary % 1_000_000).ToString("D6", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string EncodeBase32(ReadOnlySpan<byte> input)
        {
            var output = new StringBuilder((input.Length * 8 + 4) / 5);
            var buffer = 0;
            var bitsLeft = 0;

            foreach (var value in input)
            {
                buffer = (buffer << 8) | value;
                bitsLeft += 8;
                while (bitsLeft >= 5)
                {
                    output.Append(Base32Alphabet[(buffer >> (bitsLeft - 5)) & 31]);
                    bitsLeft -= 5;
                }
            }

            if (bitsLeft > 0)
                output.Append(Base32Alphabet[(buffer << (5 - bitsLeft)) & 31]);

            return output.ToString();
        }

        private static byte[] DecodeBase32(string input)
        {
            var normalized = input.Trim().TrimEnd('=').ToUpperInvariant();
            using var output = new MemoryStream();
            var buffer = 0;
            var bitsLeft = 0;

            foreach (var character in normalized)
            {
                var value = Base32Alphabet.IndexOf(character);
                if (value < 0)
                    throw new FormatException("Invalid base32 secret.");

                buffer = (buffer << 5) | value;
                bitsLeft += 5;
                if (bitsLeft >= 8)
                {
                    output.WriteByte((byte)((buffer >> (bitsLeft - 8)) & 0xff));
                    bitsLeft -= 8;
                }
            }

            return output.ToArray();
        }
    }
}
