----------------------------------Son 8 G�n� Kalan Poli�eler----------------------------------
select *
from Insurances
where IsActive=1 and
	  DATEADD(DAY, -8, InsuranceFinishDate) <= GETDATE() and
	  InsuranceFinishDate > GETDATE()
order by InsuranceFinishDate
----------------------------------------------------------------------------------------------

-------------------------Son 8 G�n� Kalan ve Mail At�lmam�� Poli�eler-------------------------
select *
from Insurances
where IsActive=1 and
	  DATEADD(DAY, -8, InsuranceFinishDate) <= GETDATE() and
	  InsuranceFinishDate > GETDATE() and
	  CONVERT(date, InsuranceLastMailDate, 101) != CONVERT(date, GETDATE(), 101)
order by InsuranceFinishDate
----------------------------------------------------------------------------------------------

--------------------------Son 8 G�n� Kalan ve Mail ATILMI� Poli�eler--------------------------
select *
from Insurances
where IsActive=1 and
	  DATEADD(DAY, -8, InsuranceFinishDate) <= GETDATE() and
	  InsuranceFinishDate > GETDATE() and
	  CONVERT(date, InsuranceLastMailDate, 101) = CONVERT(date, GETDATE(), 101)
order by InsuranceFinishDate
----------------------------------------------------------------------------------------------