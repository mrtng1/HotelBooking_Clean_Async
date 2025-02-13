using System;
using HotelBooking.Core;
using HotelBooking.UnitTests.Fakes;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;

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
        [InlineData(1, 1, -1)]  // Case 8: Start inside, end after fully booked period
        [InlineData(9, 21, -1)] // Case 6: Fully inside fully booked period
        [InlineData(1, -1, -1)] // Case 7
        [InlineData(-1, 1, -1)] // Case 7
        public async Task FindAvailableRoom_VariousScenarios_ShouldReturnExpectedRoomId(
            int fromDays, 
            int toDays, 
            int expectedRoomId)
        {
            // Arrange
            var from = startDate.AddDays(fromDays);
            var to = endDate.AddDays(toDays);

            // Act
            var roomId = await bookingManager.FindAvailableRoom(from, to);

            // Assert
            Assert.Equal(expectedRoomId, roomId);
        }
        
        /*
         *   Når der er et ledig
         *   ---
         *
         *
         *   Når der ikke er ledig rum
         *   1. Når den overlapper en anden booking
         *          a. Når perioden er indenfor booking
         *          b. Når perioden overlapper start
         *          c. Når perioden overlapper slut
         *          d. Når perioden start og slut er før eksisterende booking start og efter slut
         *
         *   2. Book tilbage i tiden
         *
         */
    }
}