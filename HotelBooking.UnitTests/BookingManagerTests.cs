using System;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

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