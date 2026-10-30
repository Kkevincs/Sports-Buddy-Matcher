namespace SportsBuddyMatcher.BusinessLogic.Interfaces;
public interface IPhotoStorage { Task<string> SaveAsync(byte[] content); }
