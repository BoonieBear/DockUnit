﻿using System;
using System.Collections.Generic;
using BoonieBear.DeckUnit.DAL;
using BoonieBear.DeckUnit.BaseType;
namespace BoonieBear.DeckUnit.DAL
{
    /// <summary>
    /// 数据库访问接口
    /// </summary>
    public interface ISqlDAL : IAlarmConfigure, ICommConfigure, ITask, IBaseConfigure, ICommandLog, IModemConfigure
    {
        void Close();
        bool LinkStatus { get; }
    }

    public interface IBaseConfigure
    {
        BaseInfo GetBaseConfigure();

    }
    public interface IAlarmConfigure
    {
        int AddAlarm(AlarmConfigure alarmConfigure);
        void UpdateAlarmConfigure(AlarmConfigure alarmConfigure);
        void DeleteAlarm(int alarmid);
        AlarmConfigure GetAlarmConfigureByID(int alarmid);
        List<AlarmConfigure> GetAlarmConfigureList(string strWhere);
    }

    public interface ICommConfigure
    {
        CommConfInfo GetCommConfInfo();
        void UpdateCommConfInfo(CommConfInfo commConf);

    }
    public interface ICommandLog
    {
        int AddLog(CommandLog commandLog);
        void DeleteLog(int id);
        CommandLog GetCommandLog(int id);
        List<CommandLog> GetLogLst(string strWhere);
    }
    public interface ITask
    {
        int AddTask(BDTask bdTask);
        void UpdateTask(BDTask bdTask);
        void DeleteTask(Int64 id);
        BDTask GetTask(Int64 id);
        List<BDTask> GetTaskLst(string strWhere);
    }
    public interface IModemConfigure
    {
        ModemConfigure GetModemConfigure();
        void UpdateModemConfigure(ModemConfigure modemConfigure);
    }
}
