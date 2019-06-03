using DbContextTests.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests
{
    [TestClass]
    public class UsingContextTest
    {
        private int userId = 2;
        private PerformanceMeter perfMeter;
        public UsingContextTest()
        {
            perfMeter = new PerformanceMeter("using-perf.csv", 1000);
        }

        [TestMethod]
        public void get_and_update_single_context()
        {
            UserTestData.PrepareUser(userId);
            string lastUsername = "";

            perfMeter.MeasurePerf(() =>
            {
                var newUsername = $"new-username-{Guid.NewGuid().ToString("n")}";
                using (var db = new MyContext())
                {
                    var user = db.Users.Find(userId);

                    if (user.UserName != newUsername)
                    {
                        user.UserName = newUsername;
                    }

                    db.SaveChanges();
                }

                lastUsername = newUsername;
            });

            using (var db = new MyContext())
            {
                var user1 = db.Users.Find(userId);
                Assert.AreEqual(lastUsername, user1.UserName);
            }
        }

        [TestMethod]
        public void get_and_update_multiple_context_reattach()
        {
            UserTestData.PrepareUser(userId);
            string lastUsername = "";

            perfMeter.MeasurePerf(() =>
            {
                var newUsername = $"new-username-{Guid.NewGuid().ToString("n")}";
                User user;

                using (var db = new MyContext())
                {
                    user = db.Users.Find(userId);
                }

                if (user.UserName != newUsername)
                {
                    user.UserName = newUsername;
                }

                using (var db = new MyContext())
                {
                    db.Users.Attach(user);
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                    user.UserName = newUsername;

                    db.SaveChanges();
                }

                lastUsername = newUsername;
            });

            using (var db = new MyContext())
            {
                var user1 = db.Users.Find(userId);
                Assert.AreEqual(lastUsername, user1.UserName);
            }
        }


        [TestMethod]
        public void get_and_add_to_collection_multiple_context_reattach()
        {
            UserTestData.PrepareUser(userId);
            int originalCount = 0;

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                originalCount = user.Orders.Count();
            }

            perfMeter.MeasurePerf(() =>
            {
                User user;

                using (var db = new MyContext())
                {
                    user = db.Users.Find(userId);
                    db.Entry(user).Collection(u => u.Orders).Load();
                }

                // if orders are not loaded, then:
                // The ObjectContext instance has been disposed and can no longer be used for operations that require a connection.
                var newOrder = new Order("item-1")
                {
                    User = user,
                    // setting user is not enough:
                    UserId = user.Id
                };
                user.Orders.Add(newOrder);

                using (var db = new MyContext())
                {
                    // the new context don't see changes in related collection (user.Orders)
                    db.Users.Attach(user);
                    db.Entry(user).State = System.Data.Entity.EntityState.Modified;

                    db.SaveChanges();
                }
            });

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                var newCount = user.Orders.Count();

                Assert.AreEqual(originalCount + perfMeter.LoopsCount, newCount);
            }
        }

        [TestMethod]
        public void get_and_add_to_collection_single_context()
        {
            UserTestData.PrepareUser(userId);
            int originalCount = 0;

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                originalCount = user.Orders.Count();
            }

            perfMeter.MeasurePerf(() =>
            {
                User user;

                using (var db = new MyContext())
                {
                    user = db.Users.Find(userId);

                    // if orders are not loaded, then:
                    // The ObjectContext instance has been disposed and can no longer be used for operations that require a connection.
                    var newOrder = new Order("item-1")
                    {
                        // no need to sest user or userid
                    };

                    // this will load all user's orders... :(
                    user.Orders.Add(newOrder);

                    db.SaveChanges();
                }
            });

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                var newCount = user.Orders.Count();

                Assert.AreEqual(originalCount + perfMeter.LoopsCount, newCount);
            }
        }

        [TestMethod]
        public void get_and_add_to_dbset_single_context()
        {
            UserTestData.PrepareUser(userId);
            int originalCount = 0;

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                originalCount = user.Orders.Count();
            }

            perfMeter.MeasurePerf(() =>
            {
                User user;

                using (var db = new MyContext())
                {
                    user = db.Users.Find(userId);

                    var newOrder = new Order("item-1")
                    {
                        User = user
                    };

                    db.Orders.Add(newOrder);

                    db.SaveChanges();
                }
            });

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                var newCount = user.Orders.Count();

                Assert.AreEqual(originalCount + perfMeter.LoopsCount, newCount);
            }
        }

        [TestMethod]
        public void get_and_add_to_dbset_multiple_contexts()
        {
            UserTestData.PrepareUser(userId);
            int originalCount = 0;

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                originalCount = user.Orders.Count();
            }

            perfMeter.MeasurePerf(() =>
            {
                User user;

                using (var db = new MyContext())
                {
                    user = db.Users.Find(userId);
                }

                var newOrder = new Order("item-1")
                {
                    // setting the user here will make EF see it as a newly added object
                    // alterntively, user can be reattached to the new cntext
                    // User = user
                    UserId = userId
                };

                using (var db = new MyContext())
                {
                    db.Orders.Add(newOrder);

                    db.SaveChanges();
                }
            });

            using (var db = new MyContext())
            {
                var user = db.Users.Find(userId);
                var newCount = user.Orders.Count();

                Assert.AreEqual(originalCount + perfMeter.LoopsCount, newCount);
            }
        }

        [TestMethod]
        public void get_and_update_multiple_context_retrieve()
        {
            UserTestData.PrepareUser(userId);
            string lastUsername = "";

            perfMeter.MeasurePerf(() =>
            {
                var newUsername = $"new-username-{Guid.NewGuid().ToString("n")}";
                User user;

                using (var db = new MyContext())
                {
                    user = db.Users.Find(userId);
                }

                if (user.UserName != newUsername)
                {
                    user.UserName = newUsername;
                }

                using (var db = new MyContext())
                {
                    var dbUser = db.Users.Find(userId);

                    dbUser.UserName = user.UserName;

                    db.SaveChanges();
                }

                lastUsername = newUsername;
            });

            using (var db = new MyContext())
            {
                var user1 = db.Users.Find(userId);
                Assert.AreEqual(lastUsername, user1.UserName);
            }
        }

        [TestMethod]
        public void get_and_update_multiple_context_bad()
        {
            UserTestData.PrepareUser(userId);

            var newUsername = $"new-username-{Guid.NewGuid().ToString("n")}";
            User user;

            using (var db = new MyContext())
            {
                user = db.Users.Find(userId);
            }

            if (user.UserName != newUsername)
            {
                user.UserName = newUsername;
            }

            using (var db = new MyContext())
            {
                user.UserName = newUsername;

                db.SaveChanges();
            }

            using (var db = new MyContext())
            {
                var user1 = db.Users.Find(userId);

                // the assingment won't do anything, since the user object is from another context
                Assert.AreNotEqual(newUsername, user1.UserName);
            }
        }
    }
}
