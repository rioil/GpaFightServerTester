using GpaFightServerTester.Models;
using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Printing;
using System.Text;
using System.Text.Json;

namespace GpaFightServerTester.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        // Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).
        public void Initialize()
        {
        }

        private readonly GpaFightApiClient _client = new GpaFightApiClient();

        public ObservableCollection<string> Logs
        {
            get => _logs ??= new();
            set => RaisePropertyChangedIfSet(ref _logs, value);
        }
        private ObservableCollection<string>? _logs;

        /// <summary>
        /// ユーザー名
        /// </summary>
        public string Username
        {
            get => _username;
            set => RaisePropertyChangedIfSet(ref _username, value);
        }
        private string _username = string.Empty;

        /// <summary>
        /// パスワード
        /// </summary>
        public string Password
        {
            get => _password;
            set => RaisePropertyChangedIfSet(ref _password, value);
        }
        private string _password = string.Empty;

        public async void RunTest()
        {
            var users = Enumerable.Range(1, 10)
                .Select(n => new User($"test{n}", 2019, "基礎工学部", "情報科学科", $"test{n}"));
            var random = new Random();
            var gpas = new string[] { "1.10", "2.20", "2.60", "3.30", "4.40", "5.00" };

            var client = new GpaFightApiClient();

            foreach (var user in users)
            {
                var createdUser = await client.CreateUser(user);
                if (createdUser is null)
                {
                    AddLog("ユーザー作成失敗");
                    //return;
                }
                else
                {
                    AddLog("ユーザー作成成功");
                }

                if (!await client.Login(new(user.Username, user.Password)))
                {
                    AddLog("ログイン失敗");
                    continue;
                    //return;
                }
                AddLog("ログイン成功");

                AddLog((await client.GetUserRaw("current"))?.Content);

                if (createdUser?.Id is not null)
                {
                    var userInfo = await client.GetUserRaw(createdUser.Id);
                    AddLog(userInfo?.Content);
                }

                if (!await client.UpdateGrade(new(gpas[random.Next() % gpas.Length])))
                {
                    AddLog("成績更新失敗");
                    //return;
                }
                else
                {
                    AddLog("成績更新成功");
                }

                var wholeRanking = await client.GetWholeRankingRaw();
                if (wholeRanking is null)
                {
                    AddLog("全体ランキング取得に失敗");
                }
                else
                {
                    AddLog(wholeRanking.ToString());
                }

                var ranking = await client.GetAffiliationRankingRaw();
                if (ranking is null)
                {
                    AddLog("所属ランキング取得に失敗");
                }
                else
                {
                    AddLog(ranking.ToString());
                    //foreach (var item in ranking.Rankings)
                    //{
                    //    AddLog("\t" + item.ToString());
                    //}
                }
            }
        }
        private ViewModelCommand? _RunTestCommand;
        public ViewModelCommand RunTestCommand => _RunTestCommand ??= new ViewModelCommand(RunTest);

        /// <summary>
        /// ユーザー作成
        /// </summary>
        public async void CreateUser()
        {
            var response = await _client.CreateUserRaw(new(Username, 2019, "基礎工学部", "情報科学科", Password));
            if (response?.IsSuccessful ?? false)
            {
                AddLog("ユーザー作成成功");
            }
            else
            {
                AddLog("ユーザー作成失敗");
            }

            AddLog(response?.Content);
        }
        private ViewModelCommand? _createUserCommand;
        public ViewModelCommand CreateUserCommand => _createUserCommand ??= new ViewModelCommand(CreateUser);

        /// <summary>
        /// ログイン
        /// </summary>
        public async void Login()
        {
            if (await _client.Login(new(Username, Password)) && await _client.GetCurrentUser())
            {
                AddLog("ログイン成功");
            }
            else
            {
                AddLog("ログイン失敗");
            }
        }
        private ViewModelCommand? _loginCommand;
        public ViewModelCommand LoginCommand => _loginCommand ??= new ViewModelCommand(Login);

        /// <summary>
        /// ユーザー削除
        /// </summary>
        public async void DeleteUser()
        {
            if (_client.CurrentUser is null)
            {
                AddLog("未ログイン");
                return;
            }

            if (await _client.DeleteUser(_client.CurrentUser))
            {
                AddLog("ユーザー削除成功");
            }
            else
            {
                AddLog("ユーザー削除失敗");
            }
        }
        private ViewModelCommand? _deleteUserCommand;
        public ViewModelCommand DeleteUserCommand => _deleteUserCommand ??= new ViewModelCommand(DeleteUser);

        public async void UpdateGrade(string gpa)
        {
            if (await _client.UpdateGrade(new(gpa)))
            {
                AddLog("成績更新成功");
            }
            else
            {
                AddLog("成績更新失敗");
            }
        }
        private ListenerCommand<string>? _updateGradeCommand;
        public ListenerCommand<string> UpdateGradeCommand => _updateGradeCommand ??= new ListenerCommand<string>(UpdateGrade);

        private void AddLog(string? message)
        {
            Logs.Add($"[{DateTime.Now}] {message}");
        }
    }
}
