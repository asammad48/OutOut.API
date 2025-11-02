using Microsoft.Extensions.Options;
using OutOut.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace OutOut.Infrastructure.Services
{
    public class OTPService
    {
        private readonly AppSettings _appSettings;
        private readonly Random _random;

        public OTPService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            _random = new Random();
        }

        private string GenerateOTP(int otpLength)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, otpLength).Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public OTPResult GenerateOTPResult()
        {
            var otp = GenerateOTP(_appSettings.OTPConfigurations.Length);
            var hashed = HashOTP(otp);

            return new OTPResult
            {
                OTP = otp,
                HashedOTP = hashed,
                RequestHistory = new List<DateTime>() { DateTime.Now }
            };
        }

        private string HashOTP(string otp)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(otp, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }

        public bool ValidateOTPHash(string totp, string hashedOTP)
        {
            if (string.IsNullOrEmpty(totp) || string.IsNullOrEmpty(hashedOTP))
                return false;

            /* Extract the bytes */
            byte[] hashBytes = Convert.FromBase64String(hashedOTP);

            /* Get the salt */
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            /* Compute the hash on the password the user entered */
            var pbkdf2 = new Rfc2898DeriveBytes(totp, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);
            /* Compare the results */
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;

            return true;
        }
    }

    public class OTPResult
    {
        public string OTP { get; init; }
        public string HashedOTP { get; init; }
        public List<DateTime> RequestHistory { get; set; }
    }
}
