# Store images in S3-compatible object storage

Brewfolio stores Coffee Bean images as private objects behind an application-owned storage interface, using an S3-compatible service rather than SQL Server or the application filesystem. MinIO supplies the local implementation while a compatible hosted provider can be selected later; this preserves provider neutrality and gives media an explicit storage boundary, at the cost of another runtime dependency and reliable cleanup for operations that cannot be transactional across SQL Server and object storage.
