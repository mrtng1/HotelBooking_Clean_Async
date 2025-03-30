Feature: Create Booking
  As a user, I want to create a booking for a room.

  Scenario: TC1 - Valid future booking
    Given there is an available room
    When I create a booking from "2025-06-10" to "2025-06-13"
    Then the booking should be successful

  Scenario: TC2 - Start date in the past
    When I create a booking from "2023-12-09" to "2023-12-11"
    Then an error "ArgumentException" should be thrown

  Scenario: TC3 - Start date is today
    When I create a booking from "2025-03-31" to "2025-04-02"
    Then the booking should be successful

  Scenario: TC4 - Start date after end date
    When I create a booking from "2024-01-10" to "2024-01-05"
    Then an error "ArgumentException" should be thrown

  Scenario: TC5 - Fully overlapping booking
    Given there is an existing booking from "2024-01-10" to "2024-01-15"
    When I create a booking from "2024-01-12" to "2024-01-14"
    Then the booking should fail

  Scenario: TC6 - Partially overlapping booking
    Given there is an existing booking from "2024-01-10" to "2024-01-15"
    When I create a booking from "2024-01-14" to "2024-01-17"
    Then the booking should fail

  Scenario: TC7 - Booking starts on existing end date
    Given there is an existing booking from "2024-01-10" to "2024-01-15"
    When I create a booking from "2024-01-15" to "2024-01-20"
    Then the booking should fail

  Scenario: TC8 - Booking ends on existing start date
    Given there is an existing booking from "2024-01-10" to "2024-01-15"
    When I create a booking from "2024-01-05" to "2024-01-10"
    Then the booking should fail

  Scenario: TC9 - Single-day booking
    Given there is an available room
    When I create a booking from "2025-07-10" to "2025-07-10"
    Then the booking should be successful

  Scenario: TC10 - Maximum length booking (30 days)
    Given there is an available room
    When I create a booking from "2025-11-10" to "2025-12-08"
    Then the booking should be successful

  Scenario: TC11 - Zero-day booking (same day)
    Given there is an available room
    When I create a booking from "2025-07-10" to "2025-07-10"
    Then the booking should be successful

  Scenario: TC12 - All rooms booked
    Given all rooms are booked from "2024-01-10" to "2024-01-15"
    When I create a booking from "2024-01-10" to "2024-01-15"
    Then the booking should fail

  Scenario: TC13 - Partially booked with available room
    Given there are 3 rooms and 2 are booked from "2025-07-10" to "2025-07-15"
    When I create a booking from "2025-07-10" to "2025-07-15"
    Then the booking should be successful

  Scenario: TC14 - Booking after existing period
    Given there is an existing booking from "2025-08-10" to "2025-08-15"
    When I create a booking from "2025-08-16" to "2025-08-20"
    Then the booking should be successful

  Scenario: TC15 - Start date is DateTime.MinValue
    When I create a booking from "0001-01-01" to "2024-01-11"
    Then an error "ArgumentException" should be thrown

#  Scenario: TC16 - Start date is DateTime.MaxValue
#    When I create a booking from "9999-12-31" to "9999-12-31"
#    Then an error "ArgumentException" should be thrown

  Scenario: TC17 - All rooms occupied by concurrent bookings
    Given all 10 rooms are booked from "2024-01-10" to "2024-01-15"
    When I create a booking from "2024-01-10" to "2024-01-15"
    Then the booking should fail

  Scenario: TC18 - Booking with gap after existing
    Given there is an existing booking from "2025-08-10" to "2025-08-15"
    When I create a booking from "2025-08-16" to "2025-08-20"
    Then the booking should be successful

#  Scenario: TC19 - Invalid date formats
#    When I create a booking with start date "2024/01/10" and end date "10-01-2024"
#    Then an error "ArgumentException" should be thrown