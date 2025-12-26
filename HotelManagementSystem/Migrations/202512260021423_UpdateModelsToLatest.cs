namespace HotelManagementSystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateModelsToLatest : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FoodOrders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReservationId = c.Int(nullable: false),
                        OrderDate = c.DateTime(nullable: false),
                        MenuItemId = c.Int(nullable: false),
                        MealType = c.String(),
                        FoodDetails = c.String(),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MenuItems", t => t.MenuItemId, cascadeDelete: true)
                .ForeignKey("dbo.Reservations", t => t.ReservationId, cascadeDelete: true)
                .Index(t => t.ReservationId)
                .Index(t => t.MenuItemId);
            
            CreateTable(
                "dbo.MenuItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Category = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        InternalCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsIncludedInStay = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Reservations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        CheckInDate = c.DateTime(nullable: false),
                        CheckOutDate = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        TotalPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ReviewComment = c.String(),
                        ReviewRating = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Rooms",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoomNumber = c.String(),
                        Floor = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        PricePerNight = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.Int(nullable: false),
                        Reservation_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Reservations", t => t.Reservation_Id)
                .Index(t => t.Reservation_Id);
            
            CreateTable(
                "dbo.SpaAppointments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReservationId = c.Int(nullable: false),
                        SpaServiceId = c.Int(nullable: false),
                        ServiceType = c.Int(nullable: false),
                        AppointmentDate = c.DateTime(nullable: false),
                        StartTime = c.Time(nullable: false, precision: 7),
                        PersonsCount = c.Int(nullable: false),
                        IsConfirmed = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Reservations", t => t.ReservationId, cascadeDelete: true)
                .ForeignKey("dbo.SpaServices", t => t.SpaServiceId, cascadeDelete: true)
                .Index(t => t.ReservationId)
                .Index(t => t.SpaServiceId);
            
            CreateTable(
                "dbo.SpaServices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        PricePerPerson = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxCapacityPerSlot = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Username = c.String(),
                        Password = c.String(),
                        FullName = c.String(),
                        Role = c.Int(nullable: false),
                        AssignedFloor = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ReservationRooms",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoomId = c.Int(nullable: false),
                        Adults = c.Int(nullable: false),
                        Children = c.Int(nullable: false),
                        ExtraBedRequested = c.Boolean(nullable: false),
                        Reservation_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Reservations", t => t.Reservation_Id)
                .ForeignKey("dbo.Rooms", t => t.RoomId, cascadeDelete: true)
                .Index(t => t.RoomId)
                .Index(t => t.Reservation_Id);
            
            DropTable("dbo.Cameras");
            DropTable("dbo.Rezervares");
            DropTable("dbo.Utilizators");
        }
        
        public override void Down()
        {
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
            
            CreateTable(
                "dbo.Rezervares",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Cameras",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.ReservationRooms", "RoomId", "dbo.Rooms");
            DropForeignKey("dbo.ReservationRooms", "Reservation_Id", "dbo.Reservations");
            DropForeignKey("dbo.Reservations", "UserId", "dbo.Users");
            DropForeignKey("dbo.SpaAppointments", "SpaServiceId", "dbo.SpaServices");
            DropForeignKey("dbo.SpaAppointments", "ReservationId", "dbo.Reservations");
            DropForeignKey("dbo.Rooms", "Reservation_Id", "dbo.Reservations");
            DropForeignKey("dbo.FoodOrders", "ReservationId", "dbo.Reservations");
            DropForeignKey("dbo.FoodOrders", "MenuItemId", "dbo.MenuItems");
            DropIndex("dbo.ReservationRooms", new[] { "Reservation_Id" });
            DropIndex("dbo.ReservationRooms", new[] { "RoomId" });
            DropIndex("dbo.SpaAppointments", new[] { "SpaServiceId" });
            DropIndex("dbo.SpaAppointments", new[] { "ReservationId" });
            DropIndex("dbo.Rooms", new[] { "Reservation_Id" });
            DropIndex("dbo.Reservations", new[] { "UserId" });
            DropIndex("dbo.FoodOrders", new[] { "MenuItemId" });
            DropIndex("dbo.FoodOrders", new[] { "ReservationId" });
            DropTable("dbo.ReservationRooms");
            DropTable("dbo.Users");
            DropTable("dbo.SpaServices");
            DropTable("dbo.SpaAppointments");
            DropTable("dbo.Rooms");
            DropTable("dbo.Reservations");
            DropTable("dbo.MenuItems");
            DropTable("dbo.FoodOrders");
        }
    }
}
