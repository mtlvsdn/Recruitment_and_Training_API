﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiClientApp.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Full_Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Company_Name { get; set; }

        public string FullName => Full_Name;
    }
}