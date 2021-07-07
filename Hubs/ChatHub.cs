using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ChatApplication.Models;
using Microsoft.AspNet.SignalR;

namespace ChatApplication.Hubs
{
    public class ChatHub : Hub
    {
        AppDbContext db = new AppDbContext();

        public void Connect(string userName)
        {
            try
            {
                var id = Context.ConnectionId;
                var count = db.Users.Where(x => x.UserName == userName).Count();
                var status = "Cevrimici";
                if (count == 0)
                {
                    db.Users.Add(new User { Id = 0, ConnectionId = id, UserName = userName, Status = status });
                    db.SaveChanges();
                }
                else
                {
                    var user = db.Users.FirstOrDefault(x => x.UserName == userName);
                    user.Status = "Cevrimici";
                    user.ConnectionId = id;

                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                // send to caller
                Clients.Caller.onConnected(id, userName, status, db.Users, db.Messages);

                // send to all except caller client
                Clients.AllExcept(id).onNewUserConnected(id, userName, status);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void SendMessageToAll(string userName, string message)
        {
            AddMessageinCache(userName, message);
            Clients.All.messageReceived(userName, message);
        }

        public void SendMessageToGruop(string userName, string message, string grupName)
        {
            AddMessageinCache(userName, message);
            Clients.Group(grupName).sendMessageToGruop(userName, message);
        }

        public void SendPrivateMessage(string toUserId, string message)
        {
            try
            {
                string fromUserId = Context.ConnectionId;
                var toUser = db.Users.FirstOrDefault(x => x.ConnectionId == toUserId);
                var fromUser = db.Users.FirstOrDefault(x => x.ConnectionId == fromUserId);

                if (toUser != null && fromUser != null)
                {
                    AddMessageinCachePrivate(toUser.UserName, fromUser.UserName, message);
                    // send to 
                    Clients.Client(toUserId.ToString()).sendPrivateMessage(fromUserId, fromUser.UserName, message);
                    // send to caller user
                    Clients.Caller.sendPrivateMessage(toUserId, fromUser.UserName, message);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            try
            {
                var id = Context.ConnectionId;
                var item = db.Users.FirstOrDefault(x => x.ConnectionId == id);
                if (item != null)
                {
                    item.Status = "Cevrimdısı";
                    item.LogoutDate = DateTime.Now;
                    db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Clients.All.onUserDisconnected(item.UserName, item.Status);
                }
                return base.OnDisconnected(stopCalled);
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

        private void AddMessageinCache(string userName, string message)
        {
            db.Messages.Add(new Message { Id = 0, SenderName = "Genel", ReceiverName = userName, MessageContent = message, Date = DateTime.Now });
            db.SaveChanges();
        }

        private void AddMessageinCachePrivate(string alici, string gonderen, string message)
        {
            db.Messages.Add(new Message { Id = 0, SenderName = alici, ReceiverName = gonderen, MessageContent = message, Date = DateTime.Now });
            db.SaveChanges();
        }

        public void join(string roomName)
        {
            Groups.Add(Context.ConnectionId, roomName);
        }
    }
}