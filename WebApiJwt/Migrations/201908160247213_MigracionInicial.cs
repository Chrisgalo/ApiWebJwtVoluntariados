namespace WebApiJwt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigracionInicial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Foundations",
                c => new
                    {
                        idFoundation = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        address = c.String(),
                        phoneNumber = c.String(),
                        person = c.String(),
                        lat = c.Double(nullable: false),
                        lng = c.Double(nullable: false),
                        url = c.String(),
                    })
                .PrimaryKey(t => t.idFoundation);
            
            CreateTable(
                "dbo.Projects",
                c => new
                    {
                        idProject = c.Int(nullable: false, identity: true),
                        name = c.String(),
                        description = c.String(),
                        startDate = c.String(),
                        endDate = c.String(),
                        quotas = c.Int(nullable: false),
                        idFoundationP = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.idProject)
                .ForeignKey("dbo.Foundations", t => t.idFoundationP, cascadeDelete: true)
                .Index(t => t.idFoundationP);
            
            CreateTable(
                "dbo.UsuarioLogins",
                c => new
                    {
                        Usuario = c.String(nullable: false, maxLength: 128),
                        Password = c.String(),
                    })
                .PrimaryKey(t => t.Usuario);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Projects", "idFoundationP", "dbo.Foundations");
            DropIndex("dbo.Projects", new[] { "idFoundationP" });
            DropTable("dbo.UsuarioLogins");
            DropTable("dbo.Projects");
            DropTable("dbo.Foundations");
        }
    }
}
