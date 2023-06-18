CREATE PROCEDURE [dbo].[SP_InsertProduct]
    @CategoryId UNIQUEIDENTIFIER NOT NULL,
    @SupplierId UNIQUEIDENTIFIER NOT NULL,
	@Name NVARCHAR(256) NOT NULL,
    @Description NVARCHAR(4000) NOT NULL,
    @Quantity INTEGER NOT NULL,
    @Price DECIMAL(8,2) NOT NULL,
    @DiscountPercentage FLOAT NULL,
    @HasDiscount BIT NOT NULL,
    @ShippingCost DECIMAL(5,2) NULL,
    @HasShipping BIT NOT NULL,
    @AverageScore FLOAT NULL,
    @SecurityStamp NVARCHAR(MAX) NOT NULL,
    @ConcurrencyStamp NVARCHAR(MAX) NOT NULL,
    @Id UNIQUEIDENTIFIER OUTPUT
AS
	DECLARE @cnt INT = 0;
    DECLARE @identity INT = 0;
    DECLARE @entityId UNIQUEIDENTIFIER = newid();

BEGIN
    SELECT @cnt = COUNT(*) FROM Products p WHERE p.Name = @Name;
    IF(@cnt > 0)
        return -1;

    INSERT INTO Products(Id,Name,Description,Quantity,Price,DiscountPercentage,HasDiscount,ShippingCost,HasShipping,AverageScore,SecurityStamp,ConcurrencyStamp)
    VALUES(@entityId,@Name,@Description,@Price,@DiscountPercentage,@HasDiscount,@ShippingCost,@HasShipping,@AverageScore,@SecurityStamp,@ConcurrencyStamp);

    SELECT @identity = SCOPE_IDENTITY();
    IF(@identity > 0)
        SELECT @Id = @entityId;
    ELSE
        RETURN -2;
END