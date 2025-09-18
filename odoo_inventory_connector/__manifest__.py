# -*- coding: utf-8 -*-
{
    'name': 'Inventory Management Connector',
    'version': '1.0',
    'summary': 'Connect to external Inventory Management system',
    'sequence': 10,
    'description': """
Inventory Management Connector
==============================
This module connects to an external Inventory Management system and imports inventory data for viewing.
Features:
    * Import inventory data using API tokens
    * View inventory details including custom fields
    * See aggregated statistics for numeric, text and boolean fields
    """,
    'category': 'Inventory',
    'author': 'Your Name',
    'website': 'https://www.example.com',
    'depends': ['base', 'web', 'mail'],
    'data': [
        'security/ir.model.access.csv',
        'views/inventory_views.xml',
        'views/field_definition_views.xml',
        'views/field_aggregation_views.xml',
        'views/field_value_views.xml',
        'views/item_views.xml',
        'views/import_wizard_views.xml',
        'views/menu_views.xml',
    ],
    'assets': {
        'web.assets_backend': [
            'odoo_inventory_connector/static/src/css/inventory_connector.css',
        ],
    },
    'demo': [],
    'installable': True,
    'application': True,
    'auto_install': False,
    'license': 'LGPL-3',
}