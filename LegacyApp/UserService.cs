﻿using System;

namespace LegacyApp
{

    public class UserService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUserCreditService _userCreditService;

        public UserService() : this(new ClientRepository(), new UserCreditService())
        {
        }

        private UserService(IClientRepository clientRepository, IUserCreditService userCreditService)
        {
            this._clientRepository = clientRepository;
            this._userCreditService = userCreditService;
        }
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!IsUserDataCorrect(firstName, lastName, email, dateOfBirth))
                return false;

            var client = _clientRepository.GetById(clientId);
            var user = CreateUser(firstName, lastName, email, dateOfBirth, client);

            if (!IsValidUserCredit(user)) return false;

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool IsUserDataCorrect(string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || !email.Contains("@") || !email.Contains("."))
                return false;

            int age = CalculateAge(dateOfBirth);
            if (age < 21) return false;
            
            return true;
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;
            return age;
        }

        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, Client client)
        {
            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName,
                HasCreditLimit = true,
                CreditLimit = 0
            };
            SetUserCreditLimit(user, client);
            return user;
        }

        private void SetUserCreditLimit(User user, Client client)
        {
            switch (client.Type)
            {
                case "VeryImportantClient":
                    user.HasCreditLimit = false;
                    break;
                case "ImportantClient":
                    int creditLimit = _userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    user.CreditLimit = creditLimit * 2;
                    break;
                default:
                    user.HasCreditLimit = true;
                    user.CreditLimit = _userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    break;
            }
        }

        private bool IsValidUserCredit(User user)
        { 
            return !(user.HasCreditLimit && user.CreditLimit < 500);
        }
    }
}