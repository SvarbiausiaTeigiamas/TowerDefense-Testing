namespace TowerDefense.Api.GameLogic.Grid
{
    public class FirstLevelArenaGrid : IArenaGrid
    {
        public GridItem[] GridItems { get; set; } = new GridItem[Constants.TowerDefense.MaxGridGridItemsForPlayer];

        public FirstLevelArenaGrid()
        {
            const string gridLayout = @"33333333
                                        33333333
                                        33333333
                                        33333333
                                        33333333
                                        33333333
                                        33333333
                                        33333333
                                        33333333";

            GridItems.CreateGrid(gridLayout);
        }

        // constructor for tests
        public FirstLevelArenaGrid(string layout, int maxNumberOfTiles = 9)
        {
            GridItems = new GridItem[maxNumberOfTiles];
            GridItems.CreateGrid(layout);
        }
    }
}
