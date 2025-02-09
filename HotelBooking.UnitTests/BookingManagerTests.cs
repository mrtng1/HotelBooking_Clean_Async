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

        //private DateTime startOccupiedDate;
        //private DateTime stopOccupiedDate;

        public BookingManagerTests(){
            // fully occupied start & end date
            DateTime start = DateTime.Today.AddDays(10);
            DateTime end = DateTime.Today.AddDays(20);
            
            //startOccupiedDate = DateTime.Today.AddDays(10);
            //stopOccupiedDate = DateTime.Today.AddDays(20);
            
            bookingRepository = new FakeBookingRepository(start, end);
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

            var bookingForReturnedRoomId = (await bookingRepository.GetAllAsync()).
                Where(b => b.RoomId == roomId
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
            DateTime from = DateTime.Today.AddDays(11);
            DateTime to = DateTime.Today.AddDays(19);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(from, to);
            // Assert
            Assert.Equal(-1, roomId);
        }
        
        [Fact]
        public async Task FindAvailableRoom_StartBeforeEndInsideFullyBooked_RoomIdMinusOne() // case 4
        {
            // Arrange
            DateTime from = DateTime.Today.AddDays(9);
            DateTime to = DateTime.Today.AddDays(19);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(from, to);
            // Assert
            Assert.Equal(-1, roomId);
        }
        
        [Fact]
        public async Task FindAvailableRoom_StartInsideEndAfterFullyBooked_RoomIdMinusOne() // case 8
        {
            // Arrange
            DateTime from = DateTime.Today.AddDays(11);
            DateTime to = DateTime.Today.AddDays(21);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(from, to);
            // Assert
            Assert.Equal(-1, roomId);
        }
        
        [Fact]
        public async Task FindAvailableRoom_InsideFullyBooked_RoomIdMinusOne() // case 6
        {
            // Arrange
            DateTime from = DateTime.Today.AddDays(9);
            DateTime to = DateTime.Today.AddDays(21);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(from, to);
            // Assert
            Assert.Equal(-1, roomId);
        }

    }
}
