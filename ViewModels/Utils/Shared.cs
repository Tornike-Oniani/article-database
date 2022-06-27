﻿using Lib.DataAccessLayer.Models;
using Lib.ViewModels.Services.Browser;
using Lib.ViewModels.Services.Dialogs;
using Lib.ViewModels.Services.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainLib.ViewModels.Utils
{
    internal sealed class Shared
    {
        // Singleton implementaion
        private Shared() { }
        private static Shared _instance;
        public static Shared GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Shared();
            }
            return _instance;
        }

        private Action<bool> _isWorkingAction;

        public IDialogService DialogService { get; private set; }
        public IWindowService WindowService { get; private set; }
        public IBrowserService BrowserService { get; private set; }
        public User User { get; private set; }

        public void SetServices(IDialogService dialogService, IWindowService windowService, IBrowserService browserService)
        {
            this.DialogService = dialogService;
            this.WindowService = windowService;
            this.BrowserService = browserService;
        }
        public void SetWorkingStatusAction(Action<bool> isWorkingAction)
        {
            this._isWorkingAction = isWorkingAction;
        }
        public void SetLoggedInUser(User user)
        {
            this.User = user;
        }

        public void IsWorking(bool isWorking)
        {
            this._isWorkingAction.Invoke(isWorking);
        }
    }
}
