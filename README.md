## ReCollect.TextUtils

This library provides HTML parsing for both Android and iOS in Xamarin Applications.
The HTML is parsed using the [Html Agility Pack](http://htmlagilitypack.codeplex.com/), 
and then emitted as rich-text for use within a mobile application.

On Android, the rich-text is emitted as a SpannableString that can be set on
a `TextView`. Whereas, on iOS, rich-text is emitted as a `NSMutableAttributedString`,
which can then be set on a `ReCollectLabel` (a subclass of `UILabel`).

This package includes a library for each platform as well as a test
application for demonstration purposes.

### Supported HTML

The following HTML is supported:
* `<h1><h1>` ... `<h5></h5>`
* `<p></p>`, `<div></div>`: Become strings with a trailing newline
* `<i></i>`, `<em></em>`: Italics
* `<b></b>`, `<strong></strong>`: Bold
* `<a href="URL"></a>`: Clickable link
* `<font></font>`
  * Attribute `color` with supported format: `#FFFFFF`, or `rgb(255,0,0)`
  * Attribute `font` with supported format: `Helvetica`

