using HotelManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelManagementSystem.Helpers
{
    public static class NotificationService
    {
        public static void Send(int userId, string msg)
        {
            using (var db = new HotelDBContext())
            {
                db.Set<Notification>().Add(new Notification
                {
                    UserId = userId,
                    Message = msg,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
                db.SaveChanges();
            }
        }
    }
}
