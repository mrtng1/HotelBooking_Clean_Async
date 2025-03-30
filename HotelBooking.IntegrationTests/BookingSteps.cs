using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Core;
using Moq;
using TechTalk.SpecFlow;
using Xunit;

namespace HotelBooking.UnitTests;

[Binding]
public class BookingSteps
{
    private BookingManager _bookingManager;
    private bool _result;
    private Exception _exception;
    private Mock<IRepository<Booking>> _mockBookingRepo;
    private Mock<IRepository<Room>> _mockRoomRepo;
    private List<Room> _rooms;
    private List<Booking> _bookings;

    [BeforeScenario]
    public void Setup()
    {
        _mockRoomRepo = new Mock<IRepository<Room>>();
        _mockBookingRepo = new Mock<IRepository<Booking>>();
        
        // Initialize mutable lists for state tracking
        _rooms = new List<Room> { new Room { Id = 1, Description = "test1"}, new Room { Id = 2, Description = "test2"} };
        _bookings = new List<Booking>();

        // Mock repository responses using the lists
        _mockRoomRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(() => _rooms);
        _mockBookingRepo.Setup(b => b.GetAllAsync()).ReturnsAsync(() => _bookings);
        _mockBookingRepo.Setup(b => b.AddAsync(It.IsAny<Booking>()))
            .Callback<Booking>(booking => _bookings.Add(booking));

        _bookingManager = new BookingManager(_mockBookingRepo.Object, _mockRoomRepo.Object);
    }

    [Given(@"there is an available room")]
    public void GivenAvailableRoom()
    {
        // handled at setup
    }
    
    [Given(@"there is an existing booking from ""(.*)"" to ""(.*)""")]
    public void GivenExistingBooking(string startDate, string endDate)
    {
        _bookings.Add(new Booking 
        { 
            StartDate = ParseDate(startDate), 
            EndDate = ParseDate(endDate),
            IsActive = true,
            RoomId = 1 // Default to first room
        });
    }

    [Given(@"there are (.*) rooms and (.*) are booked from ""(.*)"" to ""(.*)""")]
    public void GivenPartiallyBookedRooms(int totalRooms, int bookedCount, string startDate, string endDate)
    {
        // Reset rooms
        _rooms.Clear();
        _rooms.AddRange(Enumerable.Range(1, totalRooms)
            .Select(i => new Room { Id = i }));

        // Add bookings for first 'bookedCount' rooms
        _bookings.AddRange(Enumerable.Range(1, bookedCount)
            .Select(i => new Booking
            {
                StartDate = ParseDate(startDate),
                EndDate = ParseDate(endDate),
                RoomId = i,
                IsActive = true
            }));
    }

    [Given(@"all rooms are booked from ""(.*)"" to ""(.*)""")]
    public void GivenAllRoomsBooked(string startDate, string endDate)
    {
        // Book all existing rooms
        _bookings.AddRange(_rooms.Select(room => new Booking
        {
            StartDate = ParseDate(startDate),
            EndDate = ParseDate(endDate),
            RoomId = room.Id,
            IsActive = true
        }));
    }

    [Given(@"all (.*) rooms are booked from ""(.*)"" to ""(.*)""")]
    public void GivenSpecificNumberOfRoomsBooked(int roomCount, string startDate, string endDate)
    {
        // Reset rooms
        _rooms.Clear();
        _rooms.AddRange(Enumerable.Range(1, roomCount)
            .Select(i => new Room { Id = i }));

        // Book all rooms
        _bookings.AddRange(_rooms.Select(room => new Booking
        {
            StartDate = ParseDate(startDate),
            EndDate = ParseDate(endDate),
            RoomId = room.Id,
            IsActive = true
        }));
    }

    [When(@"I create a booking from ""(.*)"" to ""(.*)""")]
    public async Task WhenCreateBooking(string startDate, string endDate)
    {
        try
        {
            _result = await _bookingManager.CreateBooking(new Booking
            {
                StartDate = ParseDate(startDate),
                EndDate = ParseDate(endDate),
                RoomId = 1,
                IsActive = true,
                Customer = new Customer { Id = 1, Email = "abc@gmail.com" },
            });
        }
        catch (ArgumentException ex)
        {
            _exception = ex;
        }
    }
    
    [Then(@"the booking should be successful")]
    public void ThenBookingSuccessful()
    {
        Assert.True(_result, "Expected booking to succeed but it failed");
        Assert.Null(_exception);
    }

    [Then(@"an error ""(.*)"" should be thrown")]
    public void ThenErrorThrown(string errorType)
    {
        Assert.NotNull(_exception);
        Assert.Equal(errorType, _exception.GetType().Name);
    }

    

    [Then(@"the booking should fail")]
    public void ThenBookingShouldFail()
    {
        Assert.False(_result, "Expected booking to fail but it succeeded");
    }
    
    private DateTime ParseDate(string date) => DateTime.Parse(date.Replace("/", "-"));
}