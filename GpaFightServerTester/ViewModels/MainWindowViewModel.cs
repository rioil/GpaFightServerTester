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
using System.Text;

namespace GpaFightServerTester.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        // Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).
        public void Initialize()
        {
        }

        public ObservableCollection<string> Logs
        {
            get => _logs ??= new();
            set => RaisePropertyChangedIfSet(ref _logs, value);
        }
        private ObservableCollection<string>? _logs;

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

                AddLog(await client.GetUserRaw("current"));

                if (createdUser?.Id is not null)
                {
                    var userInfo = await client.GetUserRaw(createdUser.Id);
                    AddLog(userInfo);
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


        private void AddLog(string? message)
        {
            Logs.Add($"[{DateTime.Now}] {message}");
        }
    }
}
