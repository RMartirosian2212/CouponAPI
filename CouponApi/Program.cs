using AutoMapper;
using CouponApi;
using CouponApi.Data;
using CouponApi.Models;
using CouponApi.Models.DTO;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/coupon", (ApplicationDbContext _db, ILogger<Program> _logger) =>
{
    APIResponse response = new APIResponse();
    _logger.Log(LogLevel.Information, "Getting all Coupons");
    response.Result = _db.Coupons;
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
}).WithName("GetCoupons").Produces<APIResponse>(200).Produces(400);

app.MapGet("/api/coupon/{id:int}", (ApplicationDbContext _db, ILogger<Program> _logger, int id) =>
{
    APIResponse response = new APIResponse();
    _logger.Log(LogLevel.Information, "Getting all Coupons");
    response.Result = _db.Coupons.FirstOrDefault(x => x.Id == id);
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);
}).WithName("GetCouponById").Produces<APIResponse>(200).Produces(400);

app.MapPost("/api/coupon/", async (
    ApplicationDbContext _db,
    IMapper _mapper,
    IValidator<CouponCreateDTO> _validator,
    [FromBody] CouponCreateDTO coupon_C_DTO) =>
{
    APIResponse response = new APIResponse();

    var validationResult = await _validator.ValidateAsync(coupon_C_DTO);
    if (!validationResult.IsValid)
    {
        response.ErrorMessage.Add(validationResult.Errors.FirstOrDefault().ToString());
        return Results.BadRequest(response);
    }
    if(_db.Coupons.FirstOrDefault(x => x.Name.ToLower() == coupon_C_DTO.Name.ToLower()) != null) 
    {
        response.ErrorMessage.Add("Coupon name already exists");
        return Results.BadRequest(response);
    }


    Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);
    _db.Coupons.Add(coupon);
    _db.SaveChanges();
    CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);
    response.Result = couponDTO;
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.Created;
    return Results.Ok(response);
}).WithName("CreateCoupon").Accepts<CouponCreateDTO>("application/json").Produces<APIResponse>(201).Produces(400);


app.MapPut("/api/coupon/", async (
    ApplicationDbContext _db,
    IMapper _mapper,
    IValidator<CouponUpdateDTO> _validator,
    [FromBody] CouponUpdateDTO coupon_U_DTO) =>
{
    APIResponse response = new APIResponse();

    var validationResult = await _validator.ValidateAsync(coupon_U_DTO);
    if (validationResult == null || !validationResult.IsValid)
    {
        response.ErrorMessage.Add(validationResult?.Errors.FirstOrDefault()?.ToString() ?? "Validation failed.");
        return Results.BadRequest(response);
    }

    Coupon coupon = _db.Coupons.FirstOrDefault(x => x.Id == coupon_U_DTO.Id);
    if (coupon == null)
    {
        response.ErrorMessage.Add("Coupon not found.");
        return Results.NotFound(response);
    }
    coupon.Name = coupon_U_DTO.Name;
    coupon.Percent = coupon_U_DTO.Percent;
    coupon.IsActive = coupon_U_DTO.IsActive;
    coupon.LastUpdated = DateTime.Now;


    await _db.SaveChangesAsync();
    response.Result = _mapper.Map<CouponDTO>(coupon);
    response.IsSuccess = true;
    response.StatusCode = HttpStatusCode.OK;
    return Results.Ok(response);

}).WithName("UpdateCoupon").Accepts<CouponCreateDTO>("application/json").Produces<APIResponse>(200).Produces(400);


app.MapDelete("/api/coupon/{id:int}", (ApplicationDbContext _db, int id) =>
{
    APIResponse response = new APIResponse();
     
    Coupon coupon = _db.Coupons.FirstOrDefault(x => x.Id == id);
    if (coupon != null)
    {
        _db.Remove(coupon);
        _db.SaveChanges();
        response.IsSuccess = true;
        response.StatusCode = HttpStatusCode.OK;
        return Results.Ok(response);
    }
    else
    {
        response.ErrorMessage.Add("InvalidId");
        return Results.BadRequest(response);
    }
});


app.UseHttpsRedirection();


app.Run();
