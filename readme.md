# Kentico NewsletterMigrator #

NewsletterMigrator is a command-line utility which helps you to preserve Kentico marketing emails and templates after the upgrade to Kentico 11.

## Introduction ##

As you may have noticed in the [upgrade instructions](https://docs.kentico.com/k11/installation/upgrading-to-kentico-11#UpgradingtoKentico11-Emailmarketing "Kentico documentation"), the upgrade process does not preserve the content of marketing emails and email templates. In other words, after your application is upgraded to Kentico 11, you are no longer able to view sent marketing emails or edit drafts which had been created before the upgrade. Statistics of sent emails are still available.

In Kentico 11, the data layer of email marketing objects was significantly refactored with numerous breaking changes. For example, marketing email template content was originally divided into four parts (Style sheets, Header, Body, Footer), whereas after upgrade there is only one field where the whole email template content persists. Marketing email content structure is completely different in Kentico 11. Moreover, region placeholder format has changed, widgets and widget zones were introduced, etc. There are multiple approaches on how to recover marketing emails and templates but none is perfect. If one was selected and applied during upgrade, it wouldn't be appropriate for all customers' projects. Therefore, the upgrade utility doesn't recover marketing emails automatically.


## Before you run the utility ##

* Prepare connection strings both to Kentico DB before upgrade and upgraded Kentico DB.
    * Assure that you backed up your project and database as advised in Kentico upgrade instructions.
    * You will need to restore the database backup on your own.
    * Connection strings should also contain `Initial catalog` (ie. DB name).
* Backup the upgraded database.


## Installation ##

* Download the latest binaries from Releases.
* Unzip the archive to any location of your choice.


## Usage ##
1. Enter connection strings to `NewsletterMigrator.exe.config` file.
1. Open command prompt at the location where there archive was unzipped and run `NewsletterMigrator.exe`.


## Manual steps after migration (required) ##

* Re-sign macros on your Kentico 11 instance.
* Revise emails and make appropriate changes so that they look the same as before.
* Write your own widgets which best fit the needs of your marketers and replace the temporary ones (see [Preparing email widgets](https://docs.kentico.com/x/PQgzB "Kentico documentation") in Kentico documentation).

## Technical details ##

### How does NewsletterMigrator recover marketing email templates ###

1. Blends `TemplateHeader`, `TemplateBody`, `TemplateFooter` and `TemplateStylesheetText` column content into a single column `TemplateCode`.
1. Adds the content of `TemplateStylesheetText` as an inline styles (`<link>` tag) right before closing `</head>` tag.
1. Converts region placeholders to _widget zone_ format, i.e., `$$name:100:200$$` becomes just `$$name$$`.

### How does NewsletterMigrator recover marketing email content ###

1. Parses original email content to obtain all regions and their content.
1. Creates a _temporary_ email widget, with a single property `Code`, for each site and assigns it to all marketing email templates.
    * Output of the widget is the raw content of `Code` property.
1. Inserts one _temporary_ widget per zone in the marketing email templates and populates the `Code` property of the widget with data from the particular region of the original email.


## Notes ##
* Do not re-run the utility if it fails or doesn't terminates normally. In such case, restore upgraded DB from the backup.
* The utility doesn't modify the codebase.
* The utility accesses the database directly, i.e., not through Kentico API.
* The utility accesses following DB tables:
    * _Newsletter_NewsletterIssue_
    * _Newsletter_EmailTemplate_
    * _Newsletter_EmailWidget_
    * _Newsletter_EmailWidgetTemplate_
* The utility only works with versions 10 and 11 of Kentico database.
