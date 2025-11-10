using System;
using System.Collections.Generic;
using Cinema.DAL;
using Cinema.Entities;

namespace Cinema.BLL;

public class ShowtimeService
{
    private readonly ShowtimeRepository _showtimeRepository;

    public ShowtimeService(ShowtimeRepository showtimeRepository)
    {
        _showtimeRepository = showtimeRepository ?? throw new ArgumentNullException(nameof(showtimeRepository));
    }

    public int Create(Showtime showtime)
    {
        if (showtime is null)
        {
            throw new ArgumentNullException(nameof(showtime));
        }

        Validate(showtime);
        _showtimeRepository.Insert(showtime);
        return showtime.Id;
    }

    public bool Update(Showtime showtime)
    {
        if (showtime is null)
        {
            throw new ArgumentNullException(nameof(showtime));
        }

        Validate(showtime);
        _showtimeRepository.Update(showtime);
        return true;
    }

    public bool Delete(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id must be greater than zero.", nameof(id));
        }

        _showtimeRepository.Delete(id);
        return true;
    }

    public Showtime? GetById(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id must be greater than zero.", nameof(id));
        }

        return _showtimeRepository.GetById(id);
    }

    public IEnumerable<Showtime> GetAll()
    {
        return _showtimeRepository.GetAll();
    }

    private static void Validate(Showtime showtime)
    {
        if (showtime.StartTime >= showtime.EndTime)
        {
            throw new ArgumentException("Start time must be earlier than end time.", nameof(showtime));
        }

        if (showtime.BasePrice < 0)
        {
            throw new ArgumentException("Base price cannot be negative.", nameof(showtime));
        }
    }
}
