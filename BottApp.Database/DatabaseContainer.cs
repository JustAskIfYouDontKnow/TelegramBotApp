﻿using BottApp.Database.Message;
using BottApp.Database.User;
using Microsoft.Extensions.Logging;

namespace BottApp.Database
{

    public class DatabaseContainer : IDatabaseContainer
    {
        public IUserRepository User { get; }

        public IMessageRepository Message { get; }


        public DatabaseContainer(PostgreSqlContext db, ILoggerFactory loggerFactory)
        {
            User = new UserRepository(db, loggerFactory);
            Message = new MessageRepository(db, loggerFactory);
        }


    }
    
}
