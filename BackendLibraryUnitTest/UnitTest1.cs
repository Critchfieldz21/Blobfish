using BackendLibrary;

namespace BackendLibraryUnitTest
{
    public class UnitTest1
    {
        public static IEnumerable<object[]> TestData =>
            new List<object[]>
            {
                // Add PDF to test here
                new object[] {
                    "../../../test_pdfs/20-NE0881-W053_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "20-NE0881-W053_P2",                            // expectedFileName
                    "W053",                                         // expectedFileNamePieceMark
                    "20-NE0881",                                    // expectedProjectNumber
                    "HRP HUDSON BUILDING 1",                        // expectedProjectName
                    "W053",                                         // expectedFileContentPieceMark
                    null,                                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    44100,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/20-NE0881-W099_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "20-NE0881-W099_P2",                            // expectedFileName
                    "W099",                                         // expectedFileNamePieceMark
                    "20-NE0881",                                    // expectedProjectNumber
                    "HRP HUDSON BUILDING 1",                        // expectedProjectName
                    "W099",                                         // expectedFileContentPieceMark
                    null,                                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    54700,                                          // expectedWeight
                    "IWP2.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/20-NE0881-W184_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "20-NE0881-W184_P2",                            // expectedFileName
                    "W184",                                         // expectedFileNamePieceMark
                    "20-NE0881",                                    // expectedProjectNumber
                    "HRP HUDSON BUILDING 1",                        // expectedProjectName
                    "W184",                                         // expectedFileContentPieceMark
                    null,                                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    44000,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/24-NE1087-W003_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "24-NE1087-W003_P2",                            // expectedFileName
                    "W003",                                         // expectedFileNamePieceMark
                    "24-NE1087",                                    // expectedProjectNumber
                    "TNTR CLUBHOUSE",                               // expectedProjectName
                    "W003",                                         // expectedFileContentPieceMark
                    new string[] {"003"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    21400,                                          // expectedWeight
                    "IWP3.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/24-NE1087-W093_P1.pdf",     // filePath
                    2,                                              // expectedNumberOfPages
                    "24-NE1087-W093_P1",                            // expectedFileName
                    "W093",                                         // expectedFileNamePieceMark
                    "24-NE1087",                                    // expectedProjectNumber
                    "TNTR CLUBHOUSE",                               // expectedProjectName
                    "W093",                                         // expectedFileContentPieceMark
                    new string[] {"093"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    11000,                                          // expectedWeight
                    "WP2.00"                                        // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/24-NE1096-DT051_P0.pdf",    // filePath
                    2,                                              // expectedNumberOfPages
                    "24-NE1096-DT051_P0",                           // expectedFileName
                    "DT051",                                        // expectedFileNamePieceMark
                    "24-NE1096",                                    // expectedProjectNumber
                    "WOODBRIDGE METROPARK GARAGE",                  // expectedProjectName
                    "DT051",                                        // expectedFileContentPieceMark
                    new string[] {"206"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    65800,                                          // expectedWeight
                    "DT1.00"                                        // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1193 -W017_P2.pdf",    // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1193 -W017_P2",                           // expectedFileName
                    "W017",                                         // expectedFileNamePieceMark
                    "25-NE1193",                                    // expectedProjectNumber
                    "EAST PARK 309",                                // expectedProjectName
                    "W017",                                         // expectedFileContentPieceMark
                    new string[] {"017"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    50300,                                          // expectedWeight
                    "IWP2.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1200_W098 Rev.1_P2.pdf",// filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1200_W098 Rev.1_P2",                      // expectedFileName
                    "W098",                                         // expectedFileNamePieceMark
                    "25-NE1200",                                    // expectedProjectNumber
                    "YOURWAY PHARMA",                               // expectedProjectName
                    "W098",                                         // expectedFileContentPieceMark
                    new string[] {"102"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    51500,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1203.02-W045_P2.pdf",  // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1203.02-W045_P2",                         // expectedFileName
                    "W045",                                         // expectedFileNamePieceMark
                    "25-NE1203.02",                                 // expectedProjectNumber
                    "SPECULATIVE OFFICE / WAREHOUSE B",             // expectedProjectName
                    "W045",                                         // expectedFileContentPieceMark
                    new string[] {"2045"},                          // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    35100,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1204-W080_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1204-W080_P2",                            // expectedFileName
                    "W080",                                         // expectedFileNamePieceMark
                    "25-NE1204",                                    // expectedProjectNumber
                    "LINK - HYATT",                                 // expectedProjectName
                    "W080",                                         // expectedFileContentPieceMark
                    new string[] {"080"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    43900,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1204-W225_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1204-W225_P2",                            // expectedFileName
                    "W225",                                         // expectedFileNamePieceMark
                    "25-NE1204",                                    // expectedProjectNumber
                    "LINK - HYATT",                                 // expectedProjectName
                    "W225",                                         // expectedFileContentPieceMark
                    new string[] {"225"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    52000,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1209-W016_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1209-W016_P2",                            // expectedFileName
                    "W016",                                         // expectedFileNamePieceMark
                    "25-NE1209",                                    // expectedProjectNumber
                    "PROJECT AERIE",                                // expectedProjectName
                    "W016",                                         // expectedFileContentPieceMark
                    new string[] {"016"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    33200,                                          // expectedWeight
                    "IWP3.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1209-W207_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1209-W207_P2",                            // expectedFileName
                    "W207",                                         // expectedFileNamePieceMark
                    "25-NE1209",                                    // expectedProjectNumber
                    "PROJECT AERIE",                                // expectedProjectName
                    "W207",                                         // expectedFileContentPieceMark
                    new string[] {"214"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    11100,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W109_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W109_P2",                            // expectedFileName
                    "W109",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W109",                                         // expectedFileContentPieceMark
                    new string[] {"109"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    53396,                                          // expectedWeight
                    "SP1"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W266_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W266_P2",                            // expectedFileName
                    "W266",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W266",                                         // expectedFileContentPieceMark
                    new string[] {"266"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    12361,                                          // expectedWeight
                    "SP3"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/20-NE0881-W004_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "20-NE0881-W004_P2",                            // expectedFileName
                    "W004",                                         // expectedFileNamePieceMark
                    "20-NE0881",                                    // expectedProjectNumber
                    "HRP HUDSON BUILDING 1",                        // expectedProjectName
                    "W004",                                         // expectedFileContentPieceMark
                    null,                                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    14900,                                          // expectedWeight
                    "IWP2.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/20-NE0881-W054_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "20-NE0881-W054_P2",                            // expectedFileName
                    "W054",                                         // expectedFileNamePieceMark
                    "20-NE0881",                                    // expectedProjectNumber
                    "HRP HUDSON BUILDING 1",                        // expectedProjectName
                    "W054",                                         // expectedFileContentPieceMark
                    null,                                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    42000,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/20-NE0881-W067_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "20-NE0881-W067_P2",                            // expectedFileName
                    "W067",                                         // expectedFileNamePieceMark
                    "20-NE0881",                                    // expectedProjectNumber
                    "HRP HUDSON BUILDING 1",                        // expectedProjectName
                    "W067",                                         // expectedFileContentPieceMark
                    null,                                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    43900,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/20-NE0881-W081_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "20-NE0881-W081_P2",                            // expectedFileName
                    "W081",                                         // expectedFileNamePieceMark
                    "20-NE0881",                                    // expectedProjectNumber
                    "HRP HUDSON BUILDING 1",                        // expectedProjectName
                    "W081",                                         // expectedFileContentPieceMark
                    null,                                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    14900,                                          // expectedWeight
                    "IWP2.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/20-NE0881-W150_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "20-NE0881-W150_P2",                            // expectedFileName
                    "W150",                                         // expectedFileNamePieceMark
                    "20-NE0881",                                    // expectedProjectNumber
                    "HRP HUDSON BUILDING 1",                        // expectedProjectName
                    "W150",                                         // expectedFileContentPieceMark
                    null,                                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    41700,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/20-NE0881-W165_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "20-NE0881-W165_P2",                            // expectedFileName
                    "W165",                                         // expectedFileNamePieceMark
                    "20-NE0881",                                    // expectedProjectNumber
                    "HRP HUDSON BUILDING 1",                        // expectedProjectName
                    "W165",                                         // expectedFileContentPieceMark
                    null,                                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    50700,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1204-W034_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1204-W034_P2",                            // expectedFileName
                    "W034",                                         // expectedFileNamePieceMark
                    "25-NE1204",                                    // expectedProjectNumber
                    "LINK - HYATT",                                 // expectedProjectName
                    "W034",                                         // expectedFileContentPieceMark
                    new string[] {"034"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    54200,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1209-W210_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1209-W210_P2",                            // expectedFileName
                    "W210",                                         // expectedFileNamePieceMark
                    "25-NE1209",                                    // expectedProjectNumber
                    "PROJECT AERIE",                                // expectedProjectName
                    "W210",                                         // expectedFileContentPieceMark
                    new string[] {"217"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    11700,                                          // expectedWeight
                    "IWP1.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W260_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W260_P2",                            // expectedFileName
                    "W260",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W260",                                         // expectedFileContentPieceMark
                    new string[] {"260"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    7807,                                           // expectedWeight
                    "SP1"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1224-W144_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1224-W144_P2",                            // expectedFileName
                    "W144",                                         // expectedFileNamePieceMark
                    "25-NE1224",                                    // expectedProjectNumber
                    "MAPLETREE WAREHOUSE",                          // expectedProjectName
                    "W144",                                         // expectedFileContentPieceMark
                    new string[] {"160"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    29577,                                          // expectedWeight
                    "SP5"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W262_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W262_P2",                            // expectedFileName
                    "W262",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W262",                                         // expectedFileContentPieceMark
                    new string[] {"262"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    15263,                                          // expectedWeight
                    "SP5"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1209-W179_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1209-W179_P2",                            // expectedFileName
                    "W179",                                         // expectedFileNamePieceMark
                    "25-NE1209",                                    // expectedProjectNumber
                    "PROJECT AERIE",                                // expectedProjectName
                    "W179",                                         // expectedFileContentPieceMark
                    new string[] {"179"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    40500,                                          // expectedWeight
                    "IWP3.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W259_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W259_P2",                            // expectedFileName
                    "W259",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W259",                                         // expectedFileContentPieceMark
                    new string[] {"259"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    31173,                                          // expectedWeight
                    "SP4"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W271_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W271_P2",                            // expectedFileName
                    "W271",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W271",                                         // expectedFileContentPieceMark
                    new string[] {"271"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    12985,                                          // expectedWeight
                    "SP3"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W234_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W234_P2",                            // expectedFileName
                    "W234",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W234",                                         // expectedFileContentPieceMark
                    new string[] {"234"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    39613,                                          // expectedWeight
                    "SP2"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W218_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W218_P2",                            // expectedFileName
                    "W218",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W218",                                         // expectedFileContentPieceMark
                    new string[] {"218"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    39154,                                          // expectedWeight
                    "SP2"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W205_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W205_P2",                            // expectedFileName
                    "W205",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W205",                                         // expectedFileContentPieceMark
                    new string[] {"205"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    14416,                                          // expectedWeight
                    "SP5"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W153_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W153_P2",                            // expectedFileName
                    "W153",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W153",                                         // expectedFileContentPieceMark
                    new string[] {"153"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    24068,                                          // expectedWeight
                    "SP4"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W148_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W148_P2",                            // expectedFileName
                    "W148",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W148",                                         // expectedFileContentPieceMark
                    new string[] {"148"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    9050,                                           // expectedWeight
                    "SP5"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1212-W085_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1212-W085_P2",                            // expectedFileName
                    "W085",                                         // expectedFileNamePieceMark
                    "25-NE1212",                                    // expectedProjectNumber
                    "NEWBURGH SOUTH LOGISTICS CENTER",              // expectedProjectName
                    "W085",                                         // expectedFileContentPieceMark
                    new string[] {"085"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    57084,                                          // expectedWeight
                    "SP1"                                           // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/24-NE1096-B001_P0.pdf",     // filePath
                    2,                                              // expectedNumberOfPages
                    "24-NE1096-B001_P0",                            // expectedFileName
                    "B001",                                         // expectedFileNamePieceMark
                    "24-NE1096",                                    // expectedProjectNumber
                    "WOODBRIDGE METROPARK GARAGE",                  // expectedProjectName
                    "B001",                                         // expectedFileContentPieceMark
                    new string[] {"027","029","031","033","035","037"},// expectedControlNumbers
                    6,                                              // expectedPiecesRequired
                    50100,                                          // expectedWeight
                    "IT1.00"                                        // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/24-NE1096-DT007_P0.pdf",    // filePath
                    2,                                              // expectedNumberOfPages
                    "24-NE1096-DT007_P0",                           // expectedFileName
                    "DT007",                                        // expectedFileNamePieceMark
                    "24-NE1096",                                    // expectedProjectNumber
                    "WOODBRIDGE METROPARK GARAGE",                  // expectedProjectName
                    "DT007",                                        // expectedFileContentPieceMark
                    new string[] {"047","073","099","125","151","177"},// expectedControlNumbers
                    6,                                              // expectedPiecesRequired
                    62100,                                          // expectedWeight
                    "DT2.00"                                        // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/24-NE1096-DT003_P0.pdf",    // filePath
                    2,                                              // expectedNumberOfPages
                    "24-NE1096-DT003_P0",                           // expectedFileName
                    "DT003",                                        // expectedFileNamePieceMark
                    "24-NE1096",                                    // expectedProjectNumber
                    "WOODBRIDGE METROPARK GARAGE",                  // expectedProjectName
                    "DT003",                                        // expectedFileContentPieceMark
                    new string[] {"043","069","095","121","147"},   // expectedControlNumbers
                    5,                                              // expectedPiecesRequired
                    62500,                                          // expectedWeight
                    "DT2.00"                                        // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/24-NE1087-W133_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "24-NE1087-W133_P2",                            // expectedFileName
                    "W133",                                         // expectedFileNamePieceMark
                    "24-NE1087",                                    // expectedProjectNumber
                    "TNTR CLUBHOUSE",                               // expectedProjectName
                    "W133",                                         // expectedFileContentPieceMark
                    new string[] {"133"},                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    28800,                                          // expectedWeight
                    "IWP2.00"                                       // expectedDesignNumber
                },
                new object[] {
                    "../../../test_pdfs/25-NE1222 W017_P2.pdf",     // filePath
                    3,                                              // expectedNumberOfPages
                    "25-NE1222 W017_P2",                            // expectedFileName
                    "W017",                                         // expectedFileNamePieceMark
                    "25-NE1222",                                    // expectedProjectNumber
                    "Flint Hill",                                   // expectedProjectName
                    "W017",                                         // expectedFileContentPieceMark
                    null,                                           // expectedControlNumbers
                    1,                                              // expectedPiecesRequired
                    45300,                                          // expectedWeight
                    "IWP3.00"                                       // expectedDesignNumber
                }
            };

        [Theory]
        [MemberData(nameof(TestData))]
        public void CheckPdf(
            String filePath, 
            int expectedNumberOfPages, 
            String expectedFileName, 
            String expectedFileNamePieceMark, 
            String expectedProjectNumber, 
            String expectedProjectName, 
            String expectedFileContentPieceMark,
            String[]? expectedControlNumbers,
            int expectedPiecesRequired, 
            int expectedWeight, 
            String expectedDesignNumber)
        {
            byte[] pdfBytes = File.ReadAllBytes(filePath);
            String pdfName = Path.GetFileNameWithoutExtension(filePath);
            ShopTicket pdf = new ShopTicket(pdfName, pdfBytes);

            Assert.Multiple(
                () => Assert.Equal(expectedNumberOfPages, pdf.NumberOfPages),
                () => Assert.Equal(expectedFileName, pdf.FileName),
                () => Assert.Equal(expectedFileNamePieceMark, pdf.FileNamePieceMark),
                () => Assert.Equal(expectedProjectNumber, pdf.ProjectNumber),
                () => Assert.Equal(expectedProjectName, pdf.ProjectName),
                () => Assert.Equal(expectedFileContentPieceMark, pdf.FileContentPieceMark),
                () => Assert.Equal(expectedControlNumbers, pdf.ControlNumbers),
                () => Assert.Equal(expectedPiecesRequired, pdf.PiecesRequired),
                () => Assert.Equal(expectedWeight, pdf.Weight),
                () => Assert.Equal(expectedDesignNumber, pdf.DesignNumber));
        }
    }
}
