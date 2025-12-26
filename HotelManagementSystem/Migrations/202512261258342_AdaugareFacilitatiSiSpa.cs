namespace HotelManagementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdaugareFacilitatiSiSpa : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Reservations", "NrPersons", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Reservations", "NrPersons");
        }
    }
}
