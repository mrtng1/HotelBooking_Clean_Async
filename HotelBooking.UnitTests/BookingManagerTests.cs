using System;
using System.Collections.Generic;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using HotelBooking.Infrastructure.Repositories;
using Moq;

namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        IRepository<Booking> bookingRepository;
        
        private DateTime startDate;
        private DateTime endDate;

        public BookingManagerTests()
        {
            startDate = DateTime.Today.AddDays(10);
            endDate = DateTime.Today.AddDays(20);

            bookingRepository = new FakeBookingRepository(startDate, endDate);
            IRepository<Room> roomRepository = new FakeRoomRepository();
            bookingManager = new BookingManager(bookingRepository, roomRepository);
        }

        [Fact]
        public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException() // case 1 & 2
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Task result() => bookingManager.FindAvailableRoom(date, date);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async Task FindAvailableRoom()
        {
            
            var customers = new List<Customer>
            {
                new Customer { Id = 1, Name = "Alice Johnson", Email = "alice.johnson@example.com" },
                new Customer { Id = 2, Name = "Bob Smith", Email = "bob.smith@example.com" },
                new Customer { Id = 3, Name = "Charlie Brown", Email = "charlie.brown@example.com" },
                new Customer { Id = 4, Name = "Diana Prince", Email = "diana.prince@example.com" }
            };

            var rooms = new List<Room>
            {
                new Room { Id = 1, Description = "Standard single room with a city view" },
                new Room { Id = 2, Description = "Deluxe double room with a sea view" },
                new Room { Id = 3, Description = "Suite with a king-size bed and private balcony" },
                new Room { Id = 4, Description = "Family room with two queen-size beds and kitchenette" }
            };

            var bookings = new List<Booking>
            {
                new Booking 
                { 
                    Id = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(3), 
                    IsActive = true, CustomerId = 1, RoomId = 2, 
                    Customer = customers[0], Room = rooms[1] 
                },
                new Booking 
                { 
                    Id = 2, StartDate = DateTime.Now.AddDays(5), EndDate = DateTime.Now.AddDays(10), 
                    IsActive = false, CustomerId = 2, RoomId = 3, 
                    Customer = customers[1], Room = rooms[2] 
                },
                new Booking 
                { 
                    Id = 3, StartDate = DateTime.Now.AddDays(2), EndDate = DateTime.Now.AddDays(7), 
                    IsActive = true, CustomerId = 3, RoomId = 1, 
                    Customer = customers[2], Room = rooms[0] 
                },
                new Booking 
                { 
                    Id = 4, StartDate = DateTime.Now.AddDays(8), EndDate = DateTime.Now.AddDays(12), 
                    IsActive = false, CustomerId = 4, RoomId = 4, 
                    Customer = customers[3], Room = rooms[3] 
                }
            };

            Mock<IRepository<Room>> mockRoomRepo = new Mock<IRepository<Room>>();
            Mock<IRepository<Booking>> mockBookingRepo = new Mock<IRepository<Booking>>();
            
            // Set up the mock behavior
            mockRoomRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(rooms);
            mockBookingRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(bookings);
            
            // Act
            BookingManager roomService = new BookingManager(mockBookingRepo.Object, mockRoomRepo.Object);
            int roomId = await roomService.FindAvailableRoom(DateTime.Now.AddDays(2), DateTime.Now.AddDays(7));
            

            // Assert
            Assert.Equal(3, roomId);
            mockRoomRepo.Verify(repo => repo.GetAllAsync(), Times.Once);
        }    
            
        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne() // case 3 & 5
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
        {
            // This test was added to satisfy the following test design
            // principle: "Tests should have strong assertions".

            // Arrange
            DateTime date = DateTime.Today.AddDays(1);

            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            var bookingForReturnedRoomId = (await bookingRepository.GetAllAsync()).Where(b => b.RoomId == roomId
                && b.StartDate <= date
                && b.EndDate >= date
                && b.IsActive);

            // Assert
            Assert.Empty(bookingForReturnedRoomId);
        }

        [Fact]
        public async Task FindAvailableRoom_FullyBooked_RoomIdMinusOne() // case 7
        {
            // Arrange
            DateTime from = startDate.AddDays(1);
            DateTime to = endDate.AddDays(-1);

            // Act
            int roomId =
                await bookingManager
                    .FindAvailableRoom(from, to);

            // Assert
            Assert.Equal(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_StartBeforeEndInsideFullyBooked_RoomIdMinusOne() // case 4
        {
            // Arrange
            DateTime from = startDate.AddDays(-1);
            DateTime to = endDate.AddDays(1);

            // Act
            int roomId = await bookingManager.FindAvailableRoom(from, to);
            // Assert
            Assert.Equal(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_StartInsideEndAfterFullyBooked_RoomIdMinusOne() // case 8
        {
            // Arrange
            DateTime from = startDate.AddDays(1);
            DateTime to = endDate.AddDays(1);

            // Act
            int roomId = await bookingManager.FindAvailableRoom(from, to);
            
            // Assert
            Assert.Equal(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_InsideFullyBooked_RoomIdMinusOne() // case 6
        {
            // Arrange
            DateTime from = startDate.AddDays(9);
            DateTime to = endDate.AddDays(21);

            // Act
            int roomId = await bookingManager.FindAvailableRoom(from, to);
            // Assert
            Assert.Equal(-1, roomId);
        }

        [Theory]
        [InlineData(1, 1, false)]  // Case 8
        [InlineData(-1, 1, false)] // Case 4
        [InlineData(-1, -1, false)] // Case 6
        [InlineData(3, -3, false)] // Case 7
        [InlineData(11, 15, true)] // Case 5
        [InlineData(-7, -15, true)] // Case 3
        public async Task FindAvailableRoom_VariousScenarios_ShouldReturnExpectedRoomId(
            int fromDays, 
            int toDays, 
            bool isBookable)
        {
            // Arrange
            var from = startDate.AddDays(fromDays);
            var to = endDate.AddDays(toDays);

            // Act
            var roomId = await bookingManager.FindAvailableRoom(from, to);

            // Assert
            Assert.Equal(isBookable, roomId > 0);
        }
    }
}