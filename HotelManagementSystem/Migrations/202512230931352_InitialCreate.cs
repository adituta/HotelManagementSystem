namespace HotelManagementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Cameras",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Rezervares",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Utilizators",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NumeUtilizator = c.String(),
                        Parola = c.String(),
                        Rol = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Utilizators");
            DropTable("dbo.Rezervares");
            DropTable("dbo.Cameras");
        }
    }
}
