using Newtonsoft.Json;
using System;

namespace VirtualLand.NewSystem.Models
{
    [Serializable]
    public class RegisterBody
    {
        public string username;
        public string email;
        public string password;
        [JsonProperty("profile")]
        public ProfileBody profile;
    }

    [Serializable]
    public class LoginBody
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class ProfileBody
    {
        public string name;
        public string avatar;
    }


    [Serializable]
    public class ApiResponse<T>
    {
        public bool isSuccess;
        public int code;
        public string message;
        public T result;
        public string errors; 
    }

    [Serializable]
    public class AuthResult
    {
        public string token;
        public UserData user;
    }

    [Serializable]
    public class UserData
    {
        public int id;
        public string username;
        public string email;
        public ProfileBody profile;
    }
}