/** 
 * This file is part of the OrderImportErrorWatcher project.
 * Copyright (c) 2014 Dai Nguyen
 * Author: Dai Nguyen
**/

using OrderImportErrorWatcher.Models;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace OrderImportErrorWatcher.DataAccess
{
    public class DataService<T> : IDataService<T> where T : class
    {
        public DataContext Context { get; private set; }

        public DataService()
        {
            Context = new DataContext();
        }

        public DataService(DataContext context)
        {
            Context = context;
        }

        public async virtual Task<T> CreateAsync(T entity, CancellationToken token)
        {
            try
            {
                Context.Set<T>().Add(entity);

                if (await Context.SaveChangesAsync(token) > 0)
                    return entity;
            }
            catch { throw; }
            return null;
        }

        public async virtual Task<T> UpdateAsync(T entity, CancellationToken token)
        {
            try
            {
                Context.Entry(entity).State = System.Data.Entity.EntityState.Modified;

                if (await Context.SaveChangesAsync(token) > 0)
                    return entity;
            }
            catch { throw; }
            return null;
        }

        public async virtual Task<bool> DeleteAsync(T entity, CancellationToken token)
        {
            try
            {
                Context.Set<T>().Remove(entity);
                return await Context.SaveChangesAsync(token) > 0;
            }
            catch { throw; }
            return false;
        }

        public async virtual Task<T> GetAsync(int id, CancellationToken token)
        {
            return await Context.Set<T>().FindAsync(id, token);
        }

        public IQueryable<T> All()
        {
            return Context.Set<T>();
        }

        public async Task<bool> SendSmtpEmailAsync(SmtpConfig smtp, string subject, string body)
        {
            try
            {                
                using (MailMessage msg = new MailMessage())
                {                    
                    msg.From = new MailAddress(smtp.EmailFrom);

                    foreach (string t in smtp.EmailTos)
                    {
                        msg.To.Add(t);
                    }

                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = true;

                    using (SmtpClient client = new SmtpClient(smtp.SmtpHost, smtp.SmtpPort))
                    {
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.Credentials = new NetworkCredential(smtp.SmtpUser, smtp.SmtpPass);
                        client.EnableSsl = smtp.EnableSSL;
                        await client.SendMailAsync(msg);
                    }
                    return true;
                }
            }
            catch { throw; }
            return false;
        }
    }
}
