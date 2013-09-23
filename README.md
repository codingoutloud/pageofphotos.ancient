Page of Photos (PoP) shows multiple photos on a single page. Oh yeah.

INSTALLATION INSTRUCTIONS

- Requires that public popmedia container exists and is public at the container level
- Requires that popuploads exists and has a specific SAS called 'untrusted-uploader' with WRITE permission and is valid from anytime to 10 years from now

RUNNING

- The MVC routing table was manipulated from the default to allow for URLs like pageofphotos.com/codingoutloud which (without changes to the routing table) would not have resolved.
- The root path should work (just the domain with no path, or locally as http://127.0.0.1/)
- Click on the Upload link from home page to upload new content



