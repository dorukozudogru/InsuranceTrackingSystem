select DATEPART(yyyy,InsuranceStartDate) as Year, DATEPART(MM,InsuranceStartDate) as Month, COUNT(*)
from Insurances
where IsActive=1
group by DATEPART(yyyy,InsuranceStartDate), DATEPART(MM,InsuranceStartDate)
order by DATEPART(MM,InsuranceStartDate), DATEPART(yyyy,InsuranceStartDate)

select * from Insurances where IsActive=1 and DATEPART(yyyy,InsuranceStartDate) = 2009 order by DATEPART(MM,InsuranceStartDate)
select * from Insurances where IsActive=1 and DATEPART(yyyy,InsuranceStartDate) = 2018 order by DATEPART(MM,InsuranceStartDate)
select * from Insurances where IsActive=1 and DATEPART(yyyy,InsuranceStartDate) = 2019 order by DATEPART(MM,InsuranceStartDate)
select * from Insurances where IsActive=1 and DATEPART(yyyy,InsuranceStartDate) = 2020 order by DATEPART(MM,InsuranceStartDate)
select * from Insurances where IsActive=1 and DATEPART(yyyy,InsuranceStartDate) = 2021 order by DATEPART(MM,InsuranceStartDate)