namespace TowerDefense.Api.GameLogic.Attacks
{
    public class Attack
    {
        public List<AttackDeclaration> DirectAttackDeclarations { get; set; } = new List<AttackDeclaration>();
        public List<AttackDeclaration> ItemAttackDeclarations { get; set; } = new List<AttackDeclaration>();
    }
}
