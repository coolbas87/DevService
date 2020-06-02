using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevService.Models;

namespace DevService.RefData
{
    public sealed class RefBook
    {
        private static readonly RefBook instance = new RefBook();
        private List<Commands> commands;
        private List<Users> users;
        private List<CommandParams> commandParams;

        public static List<Commands> Commands { get => instance.commands; }
        public static List<CommandParams> CommandParams { get => instance.commandParams; }
        public static List<Users> Users { get => instance.users; }

        private void DoRefresh(MainContext context)
        {
            context.Database.OpenConnection();
            try
            {
                commands = context.Commands.FromSql("exec dbo.[dgws_GetCommands]").ToList();
                commandParams = context.CommandParams.FromSql(/*"exec dbo.[dgws_GetCommandParams]"*/"select * from dgwsCommandParams").ToList();
                users = context.Users.FromSql("exec dbo.[dgws_GetUsers]").ToList();
            }
            finally
            {
                context.Database.CloseConnection();
            }
        }

        public static void Refresh(MainContext context)
        {
            instance.DoRefresh(context);
        }
    }
}
