using IRuettae.Core.Models;
namespace IRuettae.Evaluator
{
internal partial class DatasetFactory
{
/// <summary>
/// 20 Visits, 2 Days, 2 Santas
/// 0 Desired, 0 Unavailable on day 0
/// 0 Desired, 0 Unavailable on day 1
/// </summary>
public static (OptimizationInput input, (int x, int y)[] coordinates) DataSet4()
{
	// Coordinates of points
	var coordinates = new[]
	{
		(1884,2648),
		(1531,1133),
		(1670,1104),
		(2011,2535),
		(994,471),
		(2048,1894),
		(1578,1371),
		(1658,2223),
		(2249,567),
		(3260,2702),
		(982,813),
		(1725,326),
		(2716,2248),
		(1102,731),
		(2467,1835),
		(1610,1271),
		(583,1467),
		(2434,2338),
		(3093,2535),
		(1455,1313),
		(1596,2352)
	};
	const int workingDayDuration = 9 * Hour;
	var input = new OptimizationInput
	{
		Days = new[] {(0* Hour, 0*Hour + workingDayDuration), (24* Hour, 24*Hour + workingDayDuration) },

		RouteCosts = new[,]
		{
			{ 0, 141, 1481, 852, 920, 242, 1097, 914, 2334, 635, 829, 1627, 587, 1170, 159, 1005, 1505, 2098, 195, 1220 },
			{ 141, 0, 1471, 926, 875, 282, 1119, 789, 2254, 747, 779, 1550, 679, 1081, 177, 1146, 1451, 2018, 299, 1250 },
			{ 1481, 1471, 0, 2300, 642, 1241, 471, 1982, 1260, 2006, 2227, 761, 2020, 835, 1326, 1783, 466, 1082, 1342, 453 },
			{ 852, 926, 2300, 0, 1770, 1072, 1873, 1258, 3179, 342, 745, 2474, 281, 2007, 1009, 1077, 2357, 2943, 959, 1974 },
			{ 920, 875, 642, 1770, 0, 703, 510, 1342, 1456, 1518, 1600, 756, 1499, 423, 761, 1525, 588, 1225, 830, 643 },
			{ 242, 282, 1241, 1072, 703, 0, 855, 1047, 2144, 816, 1055, 1436, 797, 1002, 104, 999, 1291, 1910, 135, 981 },
			{ 1097, 1119, 471, 1873, 510, 855, 0, 1758, 1672, 1563, 1898, 1058, 1592, 897, 953, 1314, 784, 1468, 932, 143 },
			{ 914, 789, 1982, 1258, 1342, 1047, 1758, 0, 2362, 1290, 576, 1744, 1158, 1286, 950, 1893, 1780, 2141, 1089, 1900 },
			{ 2334, 2254, 1260, 3179, 1456, 2144, 1672, 2362, 0, 2959, 2828, 708, 2922, 1174, 2184, 2948, 902, 236, 2277, 1700 },
			{ 635, 747, 2006, 342, 1518, 816, 1563, 1290, 2959, 0, 888, 2250, 145, 1802, 777, 766, 2105, 2724, 688, 1656 },
			{ 829, 779, 2227, 745, 1600, 1055, 1898, 576, 2828, 888, 0, 2162, 743, 1681, 951, 1614, 2133, 2598, 1023, 2030 },
			{ 1627, 1550, 761, 2474, 756, 1436, 1058, 1744, 708, 2250, 2162, 0, 2215, 482, 1475, 2271, 296, 473, 1569, 1124 },
			{ 587, 679, 2020, 281, 1499, 797, 1592, 1158, 2922, 145, 743, 2215, 0, 1755, 741, 900, 2087, 2686, 680, 1694 },
			{ 1170, 1081, 835, 2007, 423, 1002, 897, 1286, 1174, 1802, 1681, 482, 1755, 0, 1025, 1919, 504, 939, 1138, 1012 },
			{ 159, 177, 1326, 1009, 761, 104, 953, 950, 2184, 777, 951, 1475, 741, 1025, 0, 1045, 1348, 1948, 160, 1081 },
			{ 1005, 1146, 1783, 1077, 1525, 999, 1314, 1893, 2948, 766, 1614, 2271, 900, 1919, 1045, 0, 2045, 2727, 885, 1345 },
			{ 1505, 1451, 466, 2357, 588, 1291, 784, 1780, 902, 2105, 2133, 296, 2087, 504, 1348, 2045, 0, 687, 1417, 838 },
			{ 2098, 2018, 1082, 2943, 1225, 1910, 1468, 2141, 236, 2724, 2598, 473, 2686, 939, 1948, 2727, 687, 0, 2043, 1508 },
			{ 195, 299, 1342, 959, 830, 135, 932, 1089, 2277, 688, 1023, 1569, 680, 1138, 160, 885, 1417, 2043, 0, 1048 },
			{ 1220, 1250, 453, 1974, 643, 981, 143, 1900, 1700, 1656, 2030, 1124, 1694, 1012, 1081, 1345, 838, 1508, 1048, 0 }
		},

		Santas = new[]
		{
			new Santa { Id = 0 },
			new Santa { Id = 1 }
		},

		Visits = new[]
		{
			new Visit{Duration = 2778, Id=0,WayCostFromHome=1555, WayCostToHome=1555,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 2145, Id=1,WayCostFromHome=1558, WayCostToHome=1558,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 2871, Id=2,WayCostFromHome=169, WayCostToHome=169,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 2817, Id=3,WayCostFromHome=2351, WayCostToHome=2351,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 1446, Id=4,WayCostFromHome=771, WayCostToHome=771,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 2618, Id=5,WayCostFromHome=1313, WayCostToHome=1313,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 2280, Id=6,WayCostFromHome=481, WayCostToHome=481,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 3225, Id=7,WayCostFromHome=2112, WayCostToHome=2112,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 3301, Id=8,WayCostFromHome=1377, WayCostToHome=1377,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 1964, Id=9,WayCostFromHome=2044, WayCostToHome=2044,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 1807, Id=10,WayCostFromHome=2327, WayCostToHome=2327,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 1702, Id=11,WayCostFromHome=923, WayCostToHome=923,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 1910, Id=12,WayCostFromHome=2070, WayCostToHome=2070,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 3347, Id=13,WayCostFromHome=1000, WayCostToHome=1000,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 2414, Id=14,WayCostFromHome=1403, WayCostToHome=1403,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 2196, Id=15,WayCostFromHome=1757, WayCostToHome=1757,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 3579, Id=16,WayCostFromHome=631, WayCostToHome=631,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 2963, Id=17,WayCostFromHome=1214, WayCostToHome=1214,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 3431, Id=18,WayCostFromHome=1402, WayCostToHome=1402,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]},
			new Visit{Duration = 3494, Id=19,WayCostFromHome=412, WayCostToHome=412,Unavailable =new (int from, int to)[0],Desired = new (int from, int to)[0]}
		}
	};
	return (input, coordinates);
}
}
}
