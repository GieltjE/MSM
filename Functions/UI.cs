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
using System.Windows.Forms;

namespace MSM.Functions
{
    public static class UI
    {
        public static void ShowMessage(Control owner, String message, String header, MessageBoxIcon messageBoxIcon)
        {
            if (owner is { Created: true, IsDisposed: false, IsHandleCreated: true })
            {
                ShowMessageHelper helper = new();
                owner.Invoke(new Action<IWin32Window, String, String, MessageBoxButtons, MessageBoxIcon, MessageBoxDefaultButton>(helper.ShowMessage), owner, message, header, MessageBoxButtons.OK, messageBoxIcon, MessageBoxDefaultButton.Button1);
            }
            else
            {
                MessageBox.Show(message, header, MessageBoxButtons.OK, messageBoxIcon, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000, false);
            }
        }

        public static DialogResult AskQuestion(Form owner, String message, String header, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, MessageBoxIcon icon)
        {
            DialogResult result;

            if (owner != null && owner != new Form() && !owner.IsDisposed && owner.IsHandleCreated)
            {
                ShowMessageHelper helper = new();
                owner.Invoke(new Action<IWin32Window, String, String, MessageBoxButtons, MessageBoxIcon, MessageBoxDefaultButton>(helper.ShowMessage), owner, message, header, buttons, icon, defaultButton);
                result = helper.Result;
            }
            else
            {
                result = MessageBox.Show(message, header, buttons, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
            }
            return result;
        }
    }

    internal class ShowMessageHelper
    {
        internal DialogResult Result;
        internal void ShowMessage(IWin32Window owner, String message, String caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            Result = DialogResult.Cancel;
            Result = MessageBox.Show(owner, message, caption, buttons, icon, defaultButton, (MessageBoxOptions)0x40000);
        }
    }
}
