INSERT INTO CarBrands VALUES ('Renault')
INSERT INTO CarBrands VALUES ('Volvo')
INSERT INTO CarBrands VALUES ('Opel')
INSERT INTO CarBrands VALUES ('Peugeot')

INSERT INTO CarModels VALUES ('Clio',(SELECT Id FROM CarBrands WHERE Name = 'Renault'))
INSERT INTO CarModels VALUES ('Megane',(SELECT Id FROM CarBrands WHERE Name = 'Renault'))
INSERT INTO CarModels VALUES ('V40',(SELECT Id FROM CarBrands WHERE Name = 'Volvo'))
INSERT INTO CarModels VALUES ('S60',(SELECT Id FROM CarBrands WHERE Name = 'Volvo'))
INSERT INTO CarModels VALUES ('Corsa',(SELECT Id FROM CarBrands WHERE Name = 'Opel'))
INSERT INTO CarModels VALUES ('208',(SELECT Id FROM CarBrands WHERE Name = 'Peugeot'))
INSERT INTO CarModels VALUES ('308',(SELECT Id FROM CarBrands WHERE Name = 'Peugeot'))

INSERT INTO AspNetUsers VALUES (NEWID(), 'doruk@d.com', 'DORUK@D.COM', 'doruk@d.com', 'DORUK@D.COM', 1, 'AQAAAAEAACcQAAAAENy12qj5sVIvsWkNRVWivAYHK7Zb/7ON36sElHMpEzclKi//KanLoa5x+FzLGHIjtQ==', 'VCMTXQKQKMMPWYZSFDMJAA5MEAQS2IDZ', '7ce41431-bcbe-4060-81c6-43d331204b38', NULL, 0, 0, NULL, 1, 0)
INSERT INTO AspNetUsers VALUES (NEWID(), 'melis@d.com', 'MELIS@D.COM', 'melis@d.com', 'MELIS@D.COM', 1, 'AQAAAAEAACcQAAAAENy12qj5sVIvsWkNRVWivAYHK7Zb/7ON36sElHMpEzclKi//KanLoa5x+FzLGHIjtQ==', 'VCMTXQKQKMMPWYZSFDMJAA5MEAQS2IDZ', '7ce41431-bcbe-4060-81c6-43d331204b38', NULL, 0, 0, NULL, 1, 0)

INSERT INTO Customers VALUES ('Doruk', '?z?do?ru', '18173308618', NULL, '5334605983', NULL, 1, GETDATE(), (SELECT TOP 1 Id FROM AspNetUsers), NULL, NULL, NULL, NULL)
INSERT INTO Customers VALUES ('Bu?ra', 'Banaz', '11111111111', NULL, '5468978653', NULL, 1, GETDATE(), (SELECT TOP 1 Id FROM AspNetUsers), NULL, NULL, NULL, NULL)
INSERT INTO Customers VALUES ('Can', 'I??k', '22222222222', NULL, '5789687321', NULL, 1, GETDATE(), (SELECT TOP 1 Id FROM AspNetUsers), NULL, NULL, NULL, NULL)

INSERT INTO InsuranceCompanies VALUES ('AXA','~/Content/Site/dist/img/axa_sigorta.png')
INSERT INTO InsuranceCompanies VALUES ('GROUPAMA','~/Content/Site/dist/img/groupama.png')
INSERT INTO InsuranceCompanies VALUES ('AK S?GORTA','~/Content/Site/dist/img/ak_sigorta.png')
INSERT INTO InsuranceCompanies VALUES ('NESA S?GORTA','~/Content/Site/dist/img/default-50x50.gif')

INSERT INTO InsurancePolicies VALUES ('DASK')
INSERT INTO InsurancePolicies VALUES ('D?NEM PR?M?')
INSERT INTO InsurancePolicies VALUES ('TRAF?K')
INSERT INTO InsurancePolicies VALUES ('KASKO')
INSERT INTO InsurancePolicies VALUES ('??YER?')
INSERT INTO InsurancePolicies VALUES ('TEHL?KEL? MADDELER')
INSERT INTO InsurancePolicies VALUES ('SA?LIK')
INSERT INTO InsurancePolicies VALUES ('SEYEHAT SA?LIK')
INSERT INTO InsurancePolicies VALUES ('KONUT')
INSERT INTO InsurancePolicies VALUES ('FERD? KAZA KOLTUK')
INSERT INTO InsurancePolicies VALUES ('FERD? KAZA')