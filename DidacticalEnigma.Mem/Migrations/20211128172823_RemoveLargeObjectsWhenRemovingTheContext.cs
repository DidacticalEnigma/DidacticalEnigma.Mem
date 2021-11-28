using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DidacticalEnigma.Mem.Migrations
{
    public partial class RemoveLargeObjectsWhenRemovingTheContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                CREATE FUNCTION ""RemoveLOBWhenRemovingContextTriggerProc""()
                RETURNS TRIGGER
                language plpgsql as $$
                begin
                    IF old.""ContentObjectId"" IS NOT NULL THEN
                        PERFORM lo_unlink(old.""ContentObjectId"");
                    END IF;

                    RETURN old;
                end $$;

                CREATE TRIGGER ""RemoveLOBWhenRemovingContext"" BEFORE DELETE ON ""Contexts""
                FOR EACH ROW
                WHEN (old.""ContentObjectId"" IS NOT NULL)
                EXECUTE PROCEDURE ""RemoveLOBWhenRemovingContextTriggerProc""();
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                DROP TRIGGER ""RemoveLOBWhenRemovingContext"" ON ""Contexts"";

                DROP ROUTINE ""RemoveLOBWhenRemovingContextTriggerProc"";
                ");
        }
    }
}
