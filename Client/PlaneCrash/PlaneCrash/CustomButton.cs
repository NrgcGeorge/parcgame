﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PlaneCrash
{
    public class CustomButton : Button
    {
        public bool IsSelected { get; set; }
        public bool IsHead = false;
        public Direction HeadDirection { get; set; }
    }
}
