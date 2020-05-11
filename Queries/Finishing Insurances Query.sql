----------------------------------Son 8 Günü Kalan Poliçeler----------------------------------
select *
from Insurances
where IsActive=1 and
	  DATEADD(DAY, -8, InsuranceFinishDate) <= GETDATE() and
	  InsuranceFinishDate > GETDATE()
order by InsuranceFinishDate
----------------------------------------------------------------------------------------------

-------------------------Son 8 Günü Kalan ve Mail Atýlmamýþ Poliçeler-------------------------
select *
from Insurances
where IsActive=1 and
	  DATEADD(DAY, -8, InsuranceFinishDate) <= GETDATE() and
	  InsuranceFinishDate > GETDATE() and
	  CONVERT(date, InsuranceLastMailDate, 101) != CONVERT(date, GETDATE(), 101)
order by InsuranceFinishDate
----------------------------------------------------------------------------------------------

--------------------------Son 8 Günü Kalan ve Mail ATILMIÞ Poliçeler--------------------------
select *
from Insurances
where IsActive=1 and
	  DATEADD(DAY, -8, InsuranceFinishDate) <= GETDATE() and
	  InsuranceFinishDate > GETDATE() and
	  CONVERT(date, InsuranceLastMailDate, 101) = CONVERT(date, GETDATE(), 101)
order by InsuranceFinishDate
----------------------------------------------------------------------------------------------