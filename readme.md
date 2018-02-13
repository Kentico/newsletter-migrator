# Kentico NewsletterMigrator #

NewsletterMigrator is a command-line utility which helps you to preserve Kentico marketing emails and templates after the upgrade to Kentico 11. 

## Introduction ##

As you may have noticed the [upgrade instructions](https://docs.kentico.com/k11/installation/upgrading-to-kentico-11#UpgradingtoKentico11-Emailmarketing "Kentico documentation"), the upgrade process doesn't preserve the content of marketing emails and email templates. In the other words, after your application is upgraded to Kentico 11, you're no longer able to view sent marketing emails or edit drafts which had been created before the upgrade. However, statistics of sent emails are still available.

In Kentico 11, data layer of email marketing objects has been significantly refactored with lots of breaking changes. For example, marketing email template content was originally divided into four parts (Style sheets, Header, Body, Footer), whereas after upgrade there is only one field where the whole template content persists. And marketing email content structure is completely different in Kentico 11. Moreover, region placeholder format has changed, widgets and widget zones were introduced, etc. There are multiple approaches how to recover marketing emails and templates but no one is perfect. If one was selected and applied during upgrade, it wouldn't have to fit to all customers' projects. So that's why upgrade utility doesn't recover marketing emails automatically.


## Before you run the utility ##

* Prepare connection strings both to Kentico DB before upgrade and upgraded Kentico DB
    * Assume that you backed up your project and database as advised in Kentico upgrade instructions.
    * You'll need to restore the database backup on your own
    * Connection strings should also contain `Initial catalog` (ie. DB name)
* Backup the upgraded database.


## Installation ##

* Download latest binaries from Releases.
* Unzip the archive to any location of your choice.


## Usage ##
1. Write connection strings to `NewsletterMigrator.exe.config` file.
1. Open command prompt at location where you've unzipped the archive and run `NewsletterMigrator.exe`
 

## Manual steps after migration (required) ##

* Re-sign macros on your Kentico 11 instance.
* Revise emails and make appropriate changes so that they look the same as before.
* Write your own widgets which best fit your marketer needs and replace the temporary ones with them (see [Preparing email widgets](https://docs.kentico.com/x/PQgzB "Kentico documentation") in Kentico documentation).

## Technical details ##

### How does NewsletterMigrator recovers marketing email templates ###

1. Blends `TemplateHeader`, `TemplateBody`, `TemplateFooter` and `TemplateStylesheetText` column content into a single column `TemplateCode`.
1. Adds the content of `TemplateStylesheetText` as an inline styles (`<link>` tag) right before closing `</head>` tag.
1. Converts region placeholders to _widget zone_ format, ie. `$$name:100:200$$` becomes just `$$name$$`.

### How does NewsletterMigrator recovers marketing email content ###

1. Parses original email content in order to obtain all regions and their contents.
1. Creates a _temporary_ email widget for each site and assigns it to all marketing email templates. The _temporary_ widget has a single property (= `Code`) and works the way it just outputs raw content of its `Code` property.
1. Inserts one _temporary_ widget per zone in the marketing email template and populates the `Code` property of the widget with data from the particular region of the original email.


## Notes ##
* Do not re-run the utility if it fails or doesn't terminates normally. In such case, restore upgraded DB from the backup.
* The utility doesn't modify the codebase.
* The utility accesses the database directly, ie. not using Kentico API.
* The utility works only with the following DB tables:
    * Newsletter_NewsletterIssue
    * Newsletter_EmailTemplate
    * Newsletter_EmailWidget
    * Newsletter_EmailWidgetTemplate
* The utility only works with versions 10 and 11 of Kentico database.
