using Moq;
using TowerDefense.Api.GameLogic.Grid;
using TowerDefense.Api.GameLogic.Items.Models;
using TowerDefense.Api.GameLogic.Attacks;
using TowerDefense.Api.GameLogic.Items;

namespace TowerDefense.Api.GameLogic.Handlers.Tests
{
    public class AttackHandlerTests
    {
        private AttackHandler _sut;

        public AttackHandlerTests()
        {
            _sut = new AttackHandler();
        }

        [Fact]
        public void HandlePlayerAttacks_WithNoItems_ReturnsEmptyAttack()
        {
            string layout =
                @"333
                  333
                  333";

            var grid = new FirstLevelArenaGrid(layout);

            var result = _sut.HandlePlayerAttacks(grid, grid);

            Assert.Empty(result.DirectAttackDeclarations);
            Assert.Empty(result.ItemAttackDeclarations);
        }

        [Fact]
        public void HandlePlayerAttacks_WithOnlyBlankAndPlaceholderItems_ReturnsEmptyAttack()
        {
            string layout =
                @"333
                  333
                  333";

            var grid = new FirstLevelArenaGrid(layout);

            var result = _sut.HandlePlayerAttacks(grid, grid);

            Assert.Empty(result.DirectAttackDeclarations);
            Assert.Empty(result.ItemAttackDeclarations);
        }

        [Fact]
        public void HandlePlayerAttacks_WithItemsHavingNoAttacks_ReturnsDirectAttacks()
        {
            string layout1 = 
                @"303
                333
                333";

            string layout2 = 
                @"333
                  333
                  333";

            var grid1 = new FirstLevelArenaGrid(layout1);
            var grid2 = new FirstLevelArenaGrid(layout2);

            var result = _sut.HandlePlayerAttacks(grid1, grid2);

            Assert.Single(result.DirectAttackDeclarations);
            Assert.Empty(result.ItemAttackDeclarations);
            Assert.Equal(60, result.DirectAttackDeclarations[0].Damage);
            Assert.True(result.DirectAttackDeclarations[0].PlayerWasHit);
        }

        [Fact]
        public void HandlePlayerAttacks_WithItemsHavingAttacks_ReturnsItemAttacks()
        {
            string layout =
                @"303
                333
                333";

            var grid = new FirstLevelArenaGrid(layout);

            // Act
            var result = _sut.HandlePlayerAttacks(grid, grid);

            // Assert
            Assert.Empty(result.DirectAttackDeclarations);
            Assert.Single(result.ItemAttackDeclarations);
            Assert.Equal(60, result.ItemAttackDeclarations[0].Damage);
            Assert.False(result.ItemAttackDeclarations[0].PlayerWasHit);
        }

        [Fact]
        public void HandlePlayerAttacks_WithMixedItems_ReturnsCorrectAttacks()
        {
            const string gridLayout1 = @"33003333
                                        33330333
                                        33331333
                                        33333333
                                        33333333
                                        33333333
                                        33333333
                                        33333333
                                        33330333";

            const string gridLayout2 = @"33333333
                                        33330333
                                        33331333
                                        33333333
                                        33333333
                                        33333333
                                        33330333
                                        33333333
                                        33333333";

            var grid1 = new FirstLevelArenaGrid(gridLayout1, 72);
            var grid2 = new FirstLevelArenaGrid(gridLayout2, 72);

            var result = _sut.HandlePlayerAttacks(grid1, grid2);

            Assert.Equal(4, result.DirectAttackDeclarations.Count);
            Assert.Single(result.ItemAttackDeclarations);
            Assert.Equal(60, result.DirectAttackDeclarations[0].Damage);
            Assert.Equal(60, result.ItemAttackDeclarations[0].Damage);
        }
    }
}