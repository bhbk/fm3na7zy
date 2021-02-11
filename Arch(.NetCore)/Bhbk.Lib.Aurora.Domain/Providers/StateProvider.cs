using Bhbk.Lib.Aurora.Domain.Models;
using Bhbk.Lib.Aurora.Primitives.Enums;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bhbk.Lib.Aurora.Domain.Providers
{
    public partial class StateProvider
    {
        private List<LoginState> _state;

        public StateProvider()
        {
            _state = new List<LoginState>();
        }

        public void Add(int sessionId, Guid userId, string userName, AuthFactorType_E factor)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var login = _state.Where(x => x.SessionId == sessionId)
                .Where(x => x.UserId == userId)
                .SingleOrDefault();

            if (login != null)
                throw new ArgumentException();

            login = new LoginState
            {
                SessionId = sessionId,
                UserId = userId,
                UserName = userName,
                IsPasswordAuthComplete = false,
                IsPublicKeyAuthComplete = false,
            };

            switch (factor)
            {
                case AuthFactorType_E.PasswordOnly:
                    {
                        login.IsPasswordRequired = true;
                        login.IsPublicKeyRequired = false;
                    }
                    break;

                case AuthFactorType_E.PublicKeyOnly:
                    {
                        login.IsPasswordRequired = false;
                        login.IsPublicKeyRequired = true;
                    }
                    break;

                case AuthFactorType_E.PasswordAndPublicKey:
                    {
                        login.IsPasswordRequired = true;
                        login.IsPublicKeyRequired = true;
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            Log.Debug($"{callPath} '{login.UserName}' model:'{JsonConvert.SerializeObject(login)}'");

            _state.Add(login);
        }

        public bool AuthComplete(int sessionId, Guid userId)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var login = _state.Where(x => x.SessionId == sessionId)
                .Where(x => x.UserId == userId)
                .Single();

            Log.Debug($"{callPath} '{login.UserName}' model:'{JsonConvert.SerializeObject(login)}'");

            if (login.IsPasswordRequired == true && login.IsPasswordAuthComplete == true
                && login.IsPublicKeyRequired == true && login.IsPublicKeyAuthComplete == true)
                return true;

            if (login.IsPasswordRequired == true && login.IsPasswordAuthComplete == true
                && login.IsPublicKeyRequired == false && login.IsPublicKeyAuthComplete == false)
                return true;

            if (login.IsPasswordRequired == false && login.IsPasswordAuthComplete == false
                && login.IsPublicKeyRequired == true && login.IsPublicKeyAuthComplete == true)
                return true;

            return false;
        }

        public void AuthComplete_Password(int sessionId, Guid userId, string userPassword)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var login = _state.Where(x => x.SessionId == sessionId)
                .Where(x => x.UserId == userId)
                .Single();

            login.IsPasswordAuthComplete = true;
            login.Password = userPassword;

            Log.Debug($"{callPath} '{login.UserName}' model:'{JsonConvert.SerializeObject(login)}'");
        }

        public void AuthComplete_PublicKey(int sessionId, Guid userId)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var login = _state.Where(x => x.SessionId == sessionId)
                .Where(x => x.UserId == userId)
                .Single();

            login.IsPublicKeyAuthComplete = true;

            Log.Debug($"{callPath} '{login.UserName}' model:'{JsonConvert.SerializeObject(login)}'");
        }

        public string GetPassword(int sessionId, Guid userId)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var login = _state.Where(x => x.SessionId == sessionId)
                .Where(x => x.UserId == userId)
                .Single();

            Log.Debug($"{callPath} '{login.UserName}' model:'{JsonConvert.SerializeObject(login)}'");

            return login.Password;
        }

        public bool Remove(int sessionId, Guid userId)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var login = _state.Where(x => x.SessionId == sessionId)
                .Where(x => x.UserId == userId)
                .Single();

            Log.Debug($"{callPath} '{login.UserName}' model:'{JsonConvert.SerializeObject(login)}'");

            return _state.Remove(login);
        }
    }
}
