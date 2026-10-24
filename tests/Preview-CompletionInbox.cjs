// Isolated browser fixture for the production student notification script.
// No Firebase credentials, real students, or live writes are used.
const http = require('http');
const fs = require('fs');
const path = require('path');
const root = path.join(__dirname, '../Yarmouk/Yarmouk');
let read = false;
const report = {id:'synthetic-evaluation', title:'Your final internship report is ready', studentName:'Synthetic Student', companyName:'Synthetic Training Company', finalScore:91.2, approvedHours:160, requiredHours:160, sentAtUnixMs:Date.now()};
const master = fs.readFileSync(path.join(root,'StudentPages/Section.Master'),'utf8');
const notice = master.slice(master.indexOf('<div data-completion-notifications'),master.lastIndexOf('</body>'))
    .replace(/<%= ResolveUrl\("([^"\n]+)"\) %>/g, (_,url)=>url.replace(/^~/,''))
    .replace('<%= CompletionInboxCsrf %>','fixture');
const content = fs.readFileSync(path.join(root,'StudentPages/CompletionReports.aspx'),'utf8').match(/<section[\s\S]*<\/section>/)[0];
const server = http.createServer((req,res)=>{
    const url = new URL(req.url,'http://127.0.0.1');
    res.setHeader('Cache-Control','no-store');
    if(url.searchParams.get('action')==='list') {
        res.setHeader('Content-Type','application/json');res.end(JSON.stringify({success:true,reports:[{...report,read}]}));return;
    }
    if(url.searchParams.get('action')==='read') {
        if(req.method!=='POST'||req.headers['x-completion-csrf']!=='fixture'){res.writeHead(403);res.end();return;}
        read=true;res.setHeader('Content-Type','application/json');res.end('{"success":true}');return;
    }
    if(url.searchParams.has('pdf')) {
        res.setHeader('Content-Type','application/pdf');res.end(fs.readFileSync(path.join(__dirname,'../output/completion-tests/synthetic-completion-report.pdf')));return;
    }
    const assets = {
        '/Content/StudentPages/completion-reports.css':'text/css',
        '/Scripts/StudentPages/completion-notifications.js':'application/javascript'
    };
    if(assets[url.pathname]) {res.setHeader('Content-Type',assets[url.pathname]);res.end(fs.readFileSync(path.join(root,url.pathname)));return;}
    res.setHeader('Content-Type','text/html; charset=utf-8');
    res.end('<!doctype html><html><head><meta name="viewport" content="width=device-width,initial-scale=1"><title>Synthetic student inbox test</title><link rel="stylesheet" href="/Content/StudentPages/completion-reports.css"><style>body{font-family:Segoe UI,sans-serif;background:#f5f8f5;color:#193d2c;margin:0;padding:32px}header{max-width:1000px;margin:auto}button,a{font-family:inherit;font-size:16px}</style></head><body><header>LOCAL TEST PREVIEW — synthetic data only <a href="/StudentPages/CompletionReports">My reports<span data-completion-count></span></a></header>'+content+notice+'</body></html>');
});
server.listen(5189,'127.0.0.1',()=>console.log('Synthetic inbox preview: http://127.0.0.1:5189/StudentPages/CompletionReports'));
