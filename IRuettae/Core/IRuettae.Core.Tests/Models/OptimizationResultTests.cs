﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using IRuettae.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRuettae.Core.Models.Tests
{
    [TestClass()]
    public class OptimizationResultTests
    {
        // time intervals
        private const int hour = 3600;
        private const int minute = 60;

        // days
        private const int day1Start = 0 * hour;
        private const int day1End = 5 * hour;
        private const int day2Start = 24 * hour;
        private const int day2End = 29 * hour;

        // used ways
        private const int w01 = 1 * hour;
        private const int w12 = 4 * hour + 30 * minute;
        private const int w20 = 30 * minute;
        private const int w03 = 2 * hour;
        private const int w30 = 2 * hour;
        private const int w04 = 15 * minute;
        private const int w40 = 40 * minute;

        // durations
        private const int duration1 = 30 * minute;
        private const int duration2 = 2 * hour;
        private const int duration3 = 1 * hour;
        private const int duration4 = 20 * minute;

        // route duration
        private const int route1Duration = 2 * hour + 5 * hour + 90 * minute;
        private const int route2Duration = 5 * hour;
        private const int route3Duration = 1 * hour + 15 * minute;

        private OptimizationResult GetModel()
        {
            // unavailable
            var unavailableDay1Before = (int.MinValue, day1Start);
            var unavailableBetween = (day1End, day2Start);
            var unavailableDay2After = (day2End, int.MaxValue);

            var input = new OptimizationInput();
            {
                input.Santas = new Santa[]
                {
                    new Santa
                    {
                        Id = 100,
                    },
                    new Santa
                    {
                        Id = 200,
                    },
                };
                input.Visits = new Visit[]
                {
                    new Visit
                    {
                        Id = 0,
                        Duration = duration1,
                        Desired = new(int, int)[]
                        {
                            (day1Start, day1End),
                            (day2Start, day2End),
                        },
                        Unavailable = new(int, int)[]
                        {
                            (day1Start, day1End),
                            (day2Start, day2End),
                            unavailableDay1Before,
                            unavailableBetween,
                            unavailableDay2After,
                        },
                        IsBreak = false,
                        SantaId = -1,
                        WayCostFromHome = w01,
                        WayCostToHome = 0,
                    },
                    new Visit
                    {
                        Id = 1,
                        Duration = duration2,
                        Desired = new(int, int)[]
                        {
                            (day1Start, day1End),
                        },
                        Unavailable = new(int, int)[]
                        {
                            (day2Start, day2End),
                            unavailableDay1Before,
                            unavailableBetween,
                            unavailableDay2After,
                        },
                        IsBreak = false,
                        SantaId = -1,
                        WayCostFromHome = 0,
                        WayCostToHome = w20,
                    },
                    new Visit
                    {
                        Id = 2,
                        Duration = duration3,
                        Desired = new(int, int)[]
                        {
                            (day1Start + 2 * hour + 15 * minute, day1Start + 2 * hour + 30 * minute),
                        },
                        Unavailable = new(int, int)[]
                        {
                            (day1Start + 2 * hour + 30 * minute, day1Start + 2 * hour + 45 * minute),
                            unavailableDay1Before,
                            unavailableBetween,
                            unavailableDay2After,
                        },
                        IsBreak = false,
                        SantaId = -1,
                        WayCostFromHome = w03,
                        WayCostToHome = w30,
                    },
                    new Visit
                    {
                        Id = 3,
                        Duration = duration4,
                        Desired = new(int, int)[]
                        {
                            (day1Start, day1End),
                            (day2Start, day2End),
                        },
                        Unavailable = new(int, int)[]
                        {
                            unavailableDay1Before,
                            unavailableBetween,
                            unavailableDay2After,
                        },
                        IsBreak = false,
                        SantaId = -1,
                        WayCostFromHome = w04,
                        WayCostToHome = w40,
                    },
                    new Visit // will not be visited
                    {
                        Id = 4,
                        Duration = 100 * hour,
                        IsBreak = false,
                        SantaId = -1,
                    },
                    new Visit // break will not be done
                    {
                        Id = 5,
                        Duration = 100 * hour,
                        IsBreak = true,
                        SantaId = 100,
                    }
                };
                input.Days = new(int, int)[]
                {
                    (day1Start, day1End),
                    (day2Start, day2End),
                };
                input.RouteCosts = new int[,]
                {
                    // only w12 is really used
                    {0, w12, hour, hour, hour},
                    {hour, 0, hour, hour, hour},
                    {hour, hour, 0,hour, hour},
                    {hour, hour, hour, 0, hour},
                    {hour, hour, hour, hour, 0},
                };
            };

            var model = new OptimizationResult
            {
                OptimizationInput = input,
                Routes = new Route[]
                {
                    // day1, visits visit 1 & 2
                    new Route
                    {
                        SantaId=100,
                        Waypoints=new Waypoint[]
                        {
                            new Waypoint
                            {
                                VisitId=Constants.VisitIdHome,
                                StartTime=day1Start-2*hour,
                            },
                            // way=1h=w01
                            new Waypoint
                            {
                                VisitId=0,
                                StartTime=day1Start-1*hour,
                                // duration=30min
                                // unavailable=30min
                            },
                            // way=4.5h=w12
                            new Waypoint
                            {
                                VisitId=1,
                                StartTime=day1End-1*hour,
                                // duration=2h
                                // desired=1h
                                // unavailable=1h
                            },
                            // way=30min=w20
                            new Waypoint
                            {
                                VisitId=Constants.VisitIdHome,
                                StartTime=day1End+90*minute,
                            }
                        }
                    },
                    // day1, visits visit 3
                    // additional santa
                    new Route
                    {
                        SantaId=1000,
                        Waypoints=new Waypoint[]
                        {
                            new Waypoint
                            {
                                VisitId=Constants.VisitIdHome,
                                StartTime=day1Start,
                            },
                            // way=2h=w03
                            new Waypoint
                            {
                                VisitId=2,
                                StartTime=day1Start+2*hour,
                                // duration=1h
                                // desired=15min
                                // unavailbale=15min
                            },
                            // way=2h=w030
                            new Waypoint
                            {
                                VisitId=Constants.VisitIdHome,
                                StartTime=day1End,
                            }
                        }
                    },
                    // day2, visits visit 4
                    new Route
                    {
                        SantaId=200,
                        Waypoints=new Waypoint[]
                        {
                            new Waypoint
                            {
                                VisitId=Constants.VisitIdHome,
                                StartTime=day2Start+45*minute,
                            },
                            // way=15min=w04
                            new Waypoint
                            {
                                VisitId=3,
                                StartTime=day2Start+1*hour,
                                // duration=20min
                            },
                            // way=40min=w40
                            new Waypoint
                            {
                                VisitId=Constants.VisitIdHome,
                                StartTime=day2Start+2*hour,
                            }
                        }
                    }
                }
            };
            return model;
        }

        [TestMethod()]
        public void NumberOfNotVisitedFamiliesTest()
        {
            var model = GetModel();
            Assert.AreEqual(1, model.NumberOfNotVisitedFamilies());
        }

        [TestMethod()]
        public void NumberOfMissingBreaksTest()
        {
            var model = GetModel();
            Assert.AreEqual(1, model.NumberOfMissingBreaks());
        }

        [TestMethod()]
        public void NumberOfMissingBreaksTest_Empty()
        {
            var input = new OptimizationInput()
            {
                Santas = new Santa[]
                {
                    new Santa
                    {
                        Id = 100,
                    },
                },
                Visits = new Visit[]
                {
                    new Visit
                    {
                        Id = 0,
                        IsBreak = true,
                        SantaId = 100,
                    }
                },
                Days = new(int, int)[0],
                RouteCosts = new int[0, 0],
            };

            var model = new OptimizationResult
            {
                OptimizationInput = input,
                Routes = new Route[]
                {
                    // day1
                    // break, home, break, break, home, break
                    new Route
                    {
                        SantaId=100,
                        Waypoints=new Waypoint[]
                        {
                            new Waypoint
                            {
                                VisitId=0,
                                StartTime=day1Start-3*hour,
                            },
                            new Waypoint
                            {
                                VisitId=Constants.VisitIdHome,
                                StartTime=day1Start-2*hour,
                            },
                            new Waypoint
                            {
                                VisitId=0,
                                StartTime=day1Start-1*hour,
                            },
                            new Waypoint
                            {
                                VisitId=0,
                                StartTime=day1End-1*hour,
                            },
                            new Waypoint
                            {
                                VisitId=Constants.VisitIdHome,
                                StartTime=day1End+90*minute,
                            },
                            new Waypoint
                            {
                                VisitId=0,
                                StartTime=day1End+2*hour,
                            },
                        }
                    },
                    // day2, only one break
                    new Route
                    {
                        SantaId=100,
                        Waypoints=new Waypoint[]
                        {
                            new Waypoint
                            {
                                VisitId=0,
                                StartTime=day2Start+2*hour,
                            },
                        }
                    },
                }
            };

            Assert.AreEqual(0, model.NumberOfMissingBreaks());
        }

        [TestMethod()]
        public void NumberOfAdditionalSantasTest()
        {
            var model = GetModel();
            Assert.AreEqual(1, model.NumberOfAdditionalSantas());
        }

        [TestMethod()]
        public void AdditionalSantaWorkTimeTest()
        {
            var model = GetModel();
            Assert.AreEqual(5 * hour, model.AdditionalSantaWorkTime());
        }

        [TestMethod()]
        public void VisitTimeInUnavailabeTest()
        {
            var model = GetModel();
            var sum = 30 * minute + 1 * hour + 15 * minute;
            Assert.AreEqual(sum, model.VisitTimeInUnavailable());
        }

        [TestMethod()]
        public void WayTimeOutsideBusinessHoursTest()
        {
            var model = GetModel();
            var sum = 1 * hour + 30 * minute + 30 * minute;
            Assert.AreEqual(sum, model.WayTimeOutsideBusinessHours());
        }

        [TestMethod()]
        public void VisitTimeInDesiredTest()
        {
            var model = GetModel();
            var sum = 1 * hour + 15 * minute + 20 * minute;
            Assert.AreEqual(sum, model.VisitTimeInDesired());
        }

        [TestMethod()]
        public void SantaWorkTimeTest()
        {
            var model = GetModel();
            int r1 = 2 * hour + 5 * hour + 90 * minute;
            int r2 = 5 * hour;
            int r3 = 1 * hour + 15 * minute;
            Assert.AreEqual(r1 + r2 + r3, model.SantaWorkTime());
        }

        [TestMethod()]
        public void LongestDayTest()
        {
            var model = GetModel();
            int r1 = 2 * hour + 5 * hour + 90 * minute;
            Assert.AreEqual(r1, model.LongestDay());
        }

        [TestMethod()]
        public void NumberOfNeededSantasTest()
        {
            var model = GetModel();
            Assert.AreEqual(2, model.NumberOfNeededSantas());
        }

        [TestMethod()]
        public void NumberOfRoutesTest()
        {
            var model = GetModel();
            Assert.AreEqual(3, model.NumberOfRoutes());
        }

        [TestMethod()]
        public void NumberOfVisitsTest()
        {
            var model = GetModel();
            Assert.AreEqual(6, model.NumberOfVisits());
        }

        [TestMethod()]
        public void TotalWayTimeTest()
        {
            int sum = w01 + w12 + w20 + w03 + w30 + w04 + w40;
            var model = GetModel();
            Assert.AreEqual(sum, model.TotalWayTime());
        }

        [TestMethod()]
        public void TotalVisitTimeTest()
        {
            int sum = duration1 + duration2 + duration3 + duration4;
            var model = GetModel();
            Assert.AreEqual(sum, model.TotalVisitTime());
        }

        [TestMethod()]
        public void AverageWayTimePerRouteTest()
        {
            int avg = (int)new int[] { w01 + w12 + w20, w03 + w30, w04 + w40 }.Average();
            var model = GetModel();
            Assert.AreEqual(avg, model.AverageWayTimePerRoute());
        }

        [TestMethod()]
        public void AverageDurationPerRouteTest()
        {
            int avg = (int)new int[] { route1Duration, route2Duration, route3Duration }.Average();
            var model = GetModel();
            Assert.AreEqual(avg, model.AverageDurationPerRoute());
        }

        [TestMethod()]
        public void IsValidTest_Valid()
        {
            var model = GetModel();
            Assert.IsTrue(model.IsValid());
        }

        [TestMethod()]
        public void IsValidTest_SantaUsedTwice()
        {
            var model = GetModel();
            // santa 1000 has two routes on day 1
            model.Routes[0].SantaId = model.Routes.Skip(1).First().SantaId;

            Assert.IsFalse(model.IsValid());
        }

        [TestMethod()]
        public void IsValidTest_WrongStart()
        {
            var model = GetModel();
            // delete start in route 0
            model.Routes[0].Waypoints = model.Routes[0].Waypoints.Skip(1).ToArray();

            Assert.IsFalse(model.IsValid());
        }

        [TestMethod()]
        public void IsValidTest_WrongEnd()
        {
            var model = GetModel();
            // delete end in route 0
            model.Routes[0].Waypoints = model.Routes[0].Waypoints.Take(model.Routes[0].Waypoints.Length - 1).ToArray();

            Assert.IsFalse(model.IsValid());
        }

        [TestMethod()]
        public void IsValidTest_WrongWayFromHome()
        {
            var model = GetModel();
            // move start to that the way is one to short
            model.Routes[0].Waypoints[0].StartTime = model.Routes[0].Waypoints[1].StartTime - w01 + 1;

            Assert.IsFalse(model.IsValid());
        }

        [TestMethod()]
        public void IsValidTest_WrongWayToHome()
        {
            var model = GetModel();
            var numberOfVisits = model.Routes[0].Waypoints.Length;
            // move end to that the way is one to short
            model.Routes[0].Waypoints[numberOfVisits - 1].StartTime = model.Routes[0].Waypoints[numberOfVisits - 2].StartTime + duration2 + w20 - 1;

            Assert.IsFalse(model.IsValid());
        }

        [TestMethod()]
        public void IsValidTest_WrongWayBetween()
        {
            var model = GetModel();
            // make way from 1 to 2 too short
            model.Routes[0].Waypoints[2].StartTime = model.Routes[0].Waypoints[1].StartTime + duration1 + w12 - 1;

            Assert.IsFalse(model.IsValid());
        }
    }
}