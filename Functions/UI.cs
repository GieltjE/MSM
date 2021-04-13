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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using MSM.Data;

namespace MSM.Functions
{
    public static class UI
    {
        public static readonly SemaphoreSlim ErrorMessagesSemaphore = new(1, 1);
        public static readonly List<String> VisibleMessages = new();

#if DEBUG
        public static void ShowWarning(Form owner, String message, String header, MessageBoxIcon icon, String callerNameOverride = null, Int32 lineNumberOverride = 0, [CallerMemberName] String caller = null, [CallerLineNumber] Int32 lineNumber = 0)
        {
            message += "\r\ncaller: " + caller + "(" + callerNameOverride + ") line number: " + lineNumber + "(" + lineNumberOverride + ")";
#else
        public static void ShowWarning(Form owner, String message, String header, MessageBoxIcon icon)
        {
#endif
            ErrorMessagesSemaphore.WaitUIFriendly();

            if (VisibleMessages.Contains(message))
            {
                ErrorMessagesSemaphore.Release(1);
                return;
            }
            VisibleMessages.Add(message);
            
            UIMessage newMessage = new() { Header = header, Message = message, Icon = icon, Owner = owner };
            ThreadHelpers threadHelper = new();
            threadHelper.ExecuteThreadParameter(ShowMessage, newMessage, staThread: true);
            VisibleMessages.Remove(message);
            ErrorMessagesSemaphore.Release(1);
        }
        private static void ShowMessage(Object message)
        {
            UIMessage newMessage = (UIMessage)message;
            if (newMessage.Owner is { Created: true } && newMessage.Owner != new Form() && !newMessage.Owner.IsDisposed && newMessage.Owner.IsHandleCreated)
            {
                try
                {
                    newMessage.Owner.Invoke(new Action<Form, String, String, MessageBoxButtons, MessageBoxIcon, MessageBoxDefaultButton>(new ShowMessageHelper().ShowMessage), newMessage.Owner, newMessage.Message, newMessage.Header, MessageBoxButtons.OK, newMessage.Icon, MessageBoxDefaultButton.Button1);
                }
                // Happens when we are closing down
                catch (InvalidAsynchronousStateException) {}
            }
            else
            {
                // (MessageBoxOptions)0x40000 == force to foreground
                MessageBox.Show(newMessage.Message, newMessage.Header, MessageBoxButtons.OK, newMessage.Icon, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000, false);
                PreventKeyLeak();
            }
        }

        public static DialogResult AskQuestion(Form owner, String question, String header, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, MessageBoxIcon icon)
        {
            AskQuestion askQuestion = new() { Buttons = buttons, DefaultButton = defaultButton, Icon = icon, Message = question, Owner = owner, Header = header };
            ThreadHelpers thread = new();
            thread.ExecuteThreadParameter(AskQuestionHelper, askQuestion);

            return askQuestion.Result;
        }
        private static void AskQuestionHelper(Object input)
        {
            // This has to be performed from a thread, else the calling thread might experience weird issues if it's being forced into the STA/UI thread (don't know what happens exactly, but it sucks, breaks retail shutdown if a question is asked (e.g. are you sure you want to discard changes blabla))
            AskQuestion askQuestion = (AskQuestion)input;

            if (askQuestion.Owner != null && askQuestion.Owner != new Form() && !askQuestion.Owner.IsDisposed && askQuestion.Owner.IsHandleCreated)
            {
                ShowMessageHelper helper = new();
                askQuestion.Owner.Invoke(new Action<Form, String, String, MessageBoxButtons, MessageBoxIcon, MessageBoxDefaultButton>(helper.ShowMessage), askQuestion.Owner, askQuestion.Message, askQuestion.Header, askQuestion.Buttons, askQuestion.Icon, askQuestion.DefaultButton);
                askQuestion.Result = helper.Result;
            }
            else
            {
                if (askQuestion.Owner != null)
                {
                    askQuestion.Owner.Opacity = Statics.FormOpacityFade;
                }

                askQuestion.Result = MessageBox.Show(askQuestion.Message, askQuestion.Header, askQuestion.Buttons, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x00010000L | (MessageBoxOptions)0x00040000L); // MB_TOPMOST | MB_SETFOREGROUND
                PreventKeyLeak();
                
                if (askQuestion.Owner != null)
                {
                    askQuestion.Owner.Opacity = 1;
                }
            }
        }

        private static Byte[] GetKeyboardState()
        {
            Byte[] keyStates = new Byte[256];
            if (!NativeMethods.NativeGetKeyboardState(keyStates))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return keyStates;
        }
        private static Boolean AnyKeyPressed()
        {
            Byte[] keyState = GetKeyboardState();
            // skip the mouse buttons
            return keyState.Skip(8).Any(state => (state & 0x80) != 0);
        }
        public static void PreventKeyLeak()
        {
            UInt16 count = 0;
            try
            {
                while (AnyKeyPressed())
                {
                    // prevent hangs on broken keyboards/weird systems
                    if (count++ > 25)
                    {
                        break;
                    }

                    Application.DoEvents();
                    Thread.Sleep(Variables.ThreadAfterDoEventsSleep);
                }
            }
            catch (Exception exception)
            {
                Service.Logger.Log(Enumerations.LogTarget.General, Enumerations.LogLevel.Error, "Could not prevent key leak", exception);
            }

            // Do not use something like the following, it will cause loaderlocks (just to name a few)
            // Keys.Where(key => key != Key.None).Any(Keyboard.IsKeyDown)
            // Keyboard.GetState().GetPressedKeys().Length > 0
        }
    }

    internal struct UIMessage
    {
        public String Header;
        public String Message;
        public MessageBoxIcon Icon;
        public Form Owner;
    }
    public class AskQuestion
    {
        public Form Owner;
        public String Header;
        public String Message;
        public MessageBoxButtons Buttons;
        public MessageBoxDefaultButton DefaultButton;
        public MessageBoxIcon Icon;
        public DialogResult Result;
    }
    internal class ShowMessageHelper
    {
        internal DialogResult Result;
        internal void ShowMessage(Form owner, String message, String caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            if (owner != null)
            {
                owner.Opacity = Statics.FormOpacityFade;
            }

            Result = MessageBox.Show(owner, message, caption, buttons, icon, defaultButton, (MessageBoxOptions)0x00010000L | (MessageBoxOptions)0x00040000L); // MB_TOPMOST | MB_SETFOREGROUND
            UI.PreventKeyLeak();
            
            if (owner != null)
            {
                owner.Opacity = 1;
            }
        }
    }
}
