namespace Menulo.Application.Features.Sales.Dtos
{
    public sealed record CreateSaleDto
    (
        string FullName,
        string Username,
        string Email,
        string PhoneNumber,
        string Password,
        string ConfirmPassword
    );
}
