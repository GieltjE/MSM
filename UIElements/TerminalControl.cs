// 
// This file is a part of MSM (Multi Server Manager)
// Copyright (C) 2016-2021 Michiel Hazelhof (michiel@hazelhof.nl)
// 
// MSM is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MSM is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// If not, see <http://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using MSM.Extends;
using MSM.Service;

namespace MSM.UIElements
{
    public partial class TerminalControl : UserControlOptimized
    {
        internal AppControl AppControl;
        private readonly Server _server;
        private readonly String _uniqueID = Functions.Generate.GenerateRandomAlphaNumbericalString(20);
        public TerminalControl(Server server)
        {
            InitializeComponent();
            _server = server;

            BorderStyle = BorderStyle.None;
            Padding = new Padding(0);
        }
        ~TerminalControl()
        {
            AppControl?.Stop();
        }

        public override Int32 GetHashCode() => _uniqueID.GetHashCode();
        public override Boolean Equals(Object o) => String.Equals(((TerminalControl)o)?._uniqueID, _uniqueID, StringComparison.Ordinal);
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            //-ssh -P 222 -load "Default Settings"  -l root prolis.fyn.nl
            List<String> parameters = new()
            {
                "-ssh",
                "-P " + _server.Port.ToString(CultureInfo.InvariantCulture),
                "-load \"Default Settings\"",
            };
            if (!String.IsNullOrWhiteSpace(Settings.Values.PuttyExtraParamaters))
            {
                parameters.Insert(0, Settings.Values.PuttyExtraParamaters);
            }
            if (!String.IsNullOrEmpty(_server.Password))
            {
                parameters.Add("-pw " + _server.Password);
            }
            if (!String.IsNullOrEmpty(_server.Username))
            {
                parameters.Add("-l " + _server.Username);
            }
            parameters.Add(_server.Hostname);

            AppControl = new AppControl(this, Settings.Values.PuttyExecutable, parameters, new Dictionary<String, String>(), Handle) { Dock = DockStyle.Fill };
            Controls.Add(AppControl);
            AppControl.Load();
        }
    }
}
