using System;
using System.Configuration;
using System.Data.Common;
using System.Data.EntityClient;
using System.Security.Cryptography;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Portal.Model;

namespace Portal.Repository
{
    public class UnitOfWork : IDisposable
    {
        private readonly SuperBabyEntities _context = new SuperBabyEntities();

        private bool _disposed;
        private GenericRepository<User> _User;
        private GenericRepository<Baby> _Baby;
        private GenericRepository<TimelineEntry> _TimelineEntry;
        private GenericRepository<TypeMaster> _TypeMaster;
        private GenericRepository<User_Reset_Password> _User_Reset_Password;
      

        public UnitOfWork()
        {
            var originalConnectionString = ConfigurationManager.ConnectionStrings["SuperBabyEntities"].ConnectionString;
            var entityBuilder = new EntityConnectionStringBuilder(originalConnectionString);
            var factory = DbProviderFactories.GetFactory(entityBuilder.Provider);
            var providerBuilder = factory.CreateConnectionStringBuilder();

            providerBuilder.ConnectionString = entityBuilder.ProviderConnectionString;
            //providerBuilder.Add("Password", Password);
            entityBuilder.ProviderConnectionString = providerBuilder.ToString();

            //_context.Configuration.LazyLoadingEnabled = false;
            _context = new SuperBabyEntities(entityBuilder.ToString());
        }

        public GenericRepository<User> User
        {
            get
            {
                return _User ??
                     (_User = new GenericRepository<User>(_context));
            }
           
        }

        public GenericRepository<Baby> Baby
        {
            get
            {
                return _Baby ??
                     (_Baby = new GenericRepository<Baby>(_context));
            }

        }

        public GenericRepository<TimelineEntry> TimelineEntry
        {
            get
            {
                return 
                    _TimelineEntry??
                       (_TimelineEntry = new GenericRepository<TimelineEntry>(_context));
            }
        }

        public GenericRepository<TypeMaster> TypeMaster
        {
            get
            {
                return _TypeMaster ??
                     (_TypeMaster = new GenericRepository<TypeMaster>(_context));
            }
          
        }

        public GenericRepository<User_Reset_Password> User_Reset_Password
        {
            get
            {
                return _User_Reset_Password ??
                       (_User_Reset_Password = new GenericRepository<User_Reset_Password>(_context));
            }
        }

      

        private const string Salt = "RnJlZWRvbTE=";
        private static string _password;
        private static readonly object SyncLock = new object();
       
    

        public void SaveChanges()
        {
            _context.GetValidationErrors();
            _context.SaveChanges();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }
    }

}
