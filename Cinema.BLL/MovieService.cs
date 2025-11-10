using System;
using System.Collections.Generic;
using System.Linq;
using Cinema.Entities;

namespace Cinema.BLL;

public class MovieService
{
    private readonly IRepository<Movie> _movieRepository;

    public MovieService(IRepository<Movie> movieRepository)
    {
        _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
    }

    public int Create(Movie movie)
    {
        if (movie is null)
        {
            throw new ArgumentNullException(nameof(movie));
        }

        Validate(movie);
        _movieRepository.Insert(movie);
        return movie.Id;
    }

    public bool Update(Movie movie)
    {
        if (movie is null)
        {
            throw new ArgumentNullException(nameof(movie));
        }

        Validate(movie);
        _movieRepository.Update(movie);
        return true;
    }

    public bool Delete(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id must be greater than zero.", nameof(id));
        }

        _movieRepository.Delete(id);
        return true;
    }

    public Movie? GetById(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Id must be greater than zero.", nameof(id));
        }

        return _movieRepository.GetById(id);
    }

    public IEnumerable<Movie> GetAll()
    {
        return _movieRepository.GetAll();
    }

    public IEnumerable<Movie> Search(string? keyword, string? genre)
    {
        var movies = _movieRepository.GetAll();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var key = keyword.Trim();
            movies = movies.Where(m => m.Title.Contains(key, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            var genreKey = genre.Trim();
            movies = movies.Where(m => m.Genre.Equals(genreKey, StringComparison.OrdinalIgnoreCase));
        }

        return movies;
    }

    private static void Validate(Movie movie)
    {
        if (string.IsNullOrWhiteSpace(movie.Title))
        {
            throw new ArgumentException("Title is required.", nameof(movie));
        }

        if (movie.Duration <= 0)
        {
            throw new ArgumentException("Duration must be greater than zero.", nameof(movie));
        }

        if (movie.TicketPrice < 0)
        {
            throw new ArgumentException("Ticket price cannot be negative.", nameof(movie));
        }
    }
}
